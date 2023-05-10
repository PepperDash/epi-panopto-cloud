using System;
using System.Linq;
using System.Text;
using Crestron.Panopto.Common.ExtensionMethods;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Transports;
using Crestron.Panopto.Common.Enums;
using System.Collections.Generic;
using Crestron.Panopto.Common.Helpers;
using Crestron.Panopto.Common.Events;
using Crestron.Panopto.Common.Logging;

namespace Crestron.Panopto.Common.BasicDriver
{
    public delegate void RxOut(string message);
    public delegate ValidatedRxData ResponseValidator(string response, CommonCommandGroupType commandGroup);
    public delegate void BaseDeviceProtocolInitialized();

    public enum PowerState
    {
        Unknown = 1,
        On = 2,
        Off = 3,
        WarmingUp = 4,
        CoolingDown = 5
    }

    public abstract partial class ABaseDriverProtocol
    {
        /// <summary>
        /// A logger object that should not be referenced within drivers.
        /// This was introduced for more logging levels in the framework.
        /// </summary>
        public Logger Logger;

        /// <summary>
        /// If a driver doesn't set the polling interval, then this value will be used.
        /// </summary>
        internal const int DefaultPollingIntervalInMs = 4000;

        protected StringBuilder RxData;
        private CCriticalSection _rxDataLock;

        private bool _disposed = false;
        protected BaseRootObject DriverData;
        public string ProtocolName { get; protected set; }
        public Action<string> CustomLogger { get; set; }
        private CCriticalSection _rxEventLock = new CCriticalSection();
        private event RxOut _rxOut;
        public event RxOut RxOut
        {
            add
            {
                try
                {
                    _rxEventLock.Enter();
                    _rxOut += value;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Crestron.Panopto.Common.BasicDriver.ABaseDriverProtocol add _rxOut - {0}", e.ToString());
                }
                finally
                {
                    _rxEventLock.Leave();
                }
            }

            remove
            {
                try
                {
                    _rxEventLock.Enter();
                    _rxOut -= value;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Crestron.Panopto.Common.BasicDriver.ABaseDriverProtocol remove _rxOut - {0}", e.ToString());
                }
                finally
                {
                    _rxEventLock.Leave();
                }
            }
        }

        public bool DriverLoaded { get; protected set; }
        public bool EnableLogging { get; set; }
        public bool EnableStackTrace { get; set; }
        public bool LogTxAndRxAsBytes { get; set; }

        /// <summary>
        /// The device's current power state
        /// </summary>
        public bool PowerIsOn { get; protected set; }

        /// <summary>
        /// Returns true if the device is warming up.
        /// </summary>
        public bool WarmingUp { get; protected set; }

        /// <summary>
        /// Returns true if the device is cooling down.
        /// </summary>
        public bool CoolingDown { get; protected set; }

        /// <summary>
        /// The minimum amount of time it takes the device to warm up after a power on command.
        /// </summary>
        public uint WarmUpTime { get; set; }

        /// <summary>
        /// The minimum amount of time it takes the device to cool down after a power off command.
        /// </summary>
        public uint CoolDownTime { get; set; }

        /// <summary>
        /// No longer used but kept for previously released drivers that reference this.
        /// </summary>
        protected CTimer WarmUpCoolDownTimer;

        /// <summary>
        /// True if the driver should use the default implementation of warmup/cooldown.
        /// </summary>
        protected bool UseWarmUpCoolDownTimer;

        public bool IsConnected { get; protected set; }

        /// <summary>
        /// This is set internally when a warmup period begins.
        /// </summary>
        protected int WarmupStartTick;

        /// <summary>
        /// This is set internally when a cooldown period begins.
        /// </summary>
        protected int CooldownStartTick;

        /// <summary>
        /// Invoked when the warmup period has finished.
        /// </summary>
        protected event Action<object> WarmUpFinished;

        /// <summary>
        /// Invoked when the cooldown period has finished.
        /// </summary>
        protected event Action<object> CoolDownFinished;

        internal bool SupportsPowerFeedback;
        internal bool SupportsUnsolicitedFeedback;

        protected ResponseValidator ValidateResponse { get; set; }
        public DateTime TimeDisconnected;
        protected ATransportDriver Transport;
        protected Encoding Encoding { get; set; }
        protected uint TimeOut;
        protected int TimeoutCount;
        protected uint TimeBetweenCommands;
        protected int InternalPollingInterval { get; set; }
        protected bool SendCommands;
        protected bool EnableAutoPolling;
        protected bool PollingEnabled;
        protected bool WaitForResponse;
        protected bool QueueCommands;
        protected int MinutesUntilRemoved;
        protected CommonCommandGroupType LastCommandGroup;
        protected byte Id { set; get; }
        public int DriverID;
        private CCriticalSection _baseDeviceProtocolInitializedLock = new CCriticalSection();
        private event BaseDeviceProtocolInitialized _baseDeviceProtocolInitialized;
        public event BaseDeviceProtocolInitialized BaseDeviceProtocolInitialized
        {
            add
            {
                try
                {
                    _baseDeviceProtocolInitializedLock.Enter();
                    _baseDeviceProtocolInitialized += value;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Crestron.Panopto.Common.BasicDriver.ABaseDriverProtocol add _baseDeviceProtocolInitialized - {0}", e.ToString());
                }
                finally
                {
                    _baseDeviceProtocolInitializedLock.Leave();
                }
            }

            remove
            {
                try
                {
                    _baseDeviceProtocolInitializedLock.Enter();
                    _baseDeviceProtocolInitialized -= value;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Crestron.Panopto.Common.BasicDriver.ABaseDriverProtocol remove _baseDeviceProtocolInitialized - {0}", e.ToString());
                }
                finally
                {
                    _baseDeviceProtocolInitializedLock.Leave();
                }
            }
        }

        protected CommandSet PendingRequest;

        private CrestronQueue<string> _receivedMessages;
        private uint _receivedMessagesQueueTimeout = 5000;

        protected bool PartialOrUnrecognizedCommand;
        protected int PartialOrUnrecognizedCommandCount;
        [System.Obsolete("This is a deprecated field.", false)]
        protected int TicksBetweenPolls;
        private int _checkForTimeOutCounter;
        private CTimer _responseQueueTimer;
        private int _lastPollTick;

        private CTimer _customCommandRampTimer;
        private bool _customCommandIsRamping;
        private int _customCommandRampTicks;

        /// <summary>
        /// The original clock interval before it was changed to <see cref="DriverClock.Clock25ms"/>.
        /// </summary>
        private static uint _originalClockInterval = 250;

        /// <summary>
        /// Clock is 25ms.
        /// To get 1 second: 1000ms/25ms = 40 ticks
        /// </summary>
        private uint _oneSecondInClockTicks = 1000 / DriverClock.Clock25ms;

        /// <summary>
        /// Set to true when ever the driver receives data.
        /// This will be set to false by <see cref="PollForConnectionState"\> when the connection 
        /// poll command is sent.
        /// </summary>
        private bool _connectionPollCommandReceivedResponse;

        /// <summary>
        /// Time in ms between polls.  Must be less than or equal to 1000 and a multiple of TickRate (1000).
        /// </summary>
        public int PollingInterval
        {
            set
            {
                var newPollingInterval = value;
                if (newPollingInterval < 1000)
                    newPollingInterval = 1000;

                InternalPollingInterval = newPollingInterval;
            }
            get { return InternalPollingInterval; }
        }

        /// <summary>
        /// The constructor for the base driver protocol. 
        /// </summary>
        /// <param name="transport">The transport being used for communcation.</param>
        /// <param name="id">The device ID used in communication if the API requires an ID embedded in commands.</param>
        public ABaseDriverProtocol(ISerialTransport transport, byte id)
        {
            Id = id;
            Transport = transport as ATransportDriver;
            Transport.DataHandler = DataHandler;
            Transport.MessageTimedOut = MessageTimedOut;
            Transport.ConnectionChanged = ConnectionChanged;

            QueueCommands = true;
            LogTxAndRxAsBytes = true;
            SendCommands = true;
            MinutesUntilRemoved = 10;
            TimeoutCount = 0;
            PollingInterval = DefaultPollingIntervalInMs;
            Encoding = Encoding.GetEncoding("ISO-8859-1");

            RxData = new StringBuilder();
            _rxDataLock = new CCriticalSection();

            PowerIsOn = false;
            WarmingUp = false;
            CoolingDown = false;
            IsConnected = false;

            _checkForTimeOutCounter = 0;
            _receivedMessages = new CrestronQueue<string>(16000);
            _responseQueueTimer = new CTimer(ProcessReceivedMessages, 0);
        }

        /// <summary>
        /// Called by every device-type thorugh their own Initialize method. 
        /// <para>This may not be called by drivers.</para>
        /// </summary>
        /// <param name="driverData">Object of type <see cref="BaseRootObject"/></param>
        public virtual void Initialize(object driverData)
        {
            if (driverData != null &&
                driverData is BaseRootObject)
            {
                DriverData = (BaseRootObject)driverData;
                try
                {
                    TimeOut = DriverData.CrestronSerialDeviceApi.Api.Communication.ResponseTimeout;
                    EnableAutoPolling = DriverData.CrestronSerialDeviceApi.Api.Communication.EnableAutoPolling;

                    if (DriverData.CrestronSerialDeviceApi.Api.PowerWaitPeriod.Exists())
                    {
                        WarmUpTime = DriverData.CrestronSerialDeviceApi.Api.PowerWaitPeriod.WarmUpTime;
                        CoolDownTime = DriverData.CrestronSerialDeviceApi.Api.PowerWaitPeriod.CoolDownTime;

                        if (EnableLogging)
                        {
                            Log(string.Format("Set warmup and cooldown times to: Warmup: {0} - Cooldown: {1}",
                                WarmUpTime, CoolDownTime));
                        }
                    }
                    else if (EnableLogging)
                    {
                        Log("Warmup and cooldown times not set due to missing property driver's JSON data: DisplayData.CrestronSerialDeviceApi.Api.PowerWaitPeriod");
                    }

                    // If TimeBetweenCommandsOverride is set, then use it.
                    // Otherwise default to TimeBetweenCommands where for backward compatibility 
                    // we calculate it to be a multiple of 250ms
                    if (DriverData.CrestronSerialDeviceApi.Api.Communication.TimeBetweenCommandsOverride == null)
                    {
                        var jsonValue = DriverData.CrestronSerialDeviceApi.Api.Communication.TimeBetweenCommands;
                        if (jsonValue == 0)
                        {
                            // Zero is not valid, set to the original clock interval
                            TimeBetweenCommands = _originalClockInterval;
                        }
                        else
                        {
                            // Value must be a multiple of the original clock interval (250ms). 
                            // If it is not, then it must be set to the next value possible
                            // Example 251 would be translated to 500.
                            TimeBetweenCommands = (uint)Math.Ceiling(
                                jsonValue / (double)_originalClockInterval) * _originalClockInterval;
                        }
                    }
                    else
                    {
                        var jsonValue = (uint)DriverData.CrestronSerialDeviceApi.Api.Communication.TimeBetweenCommandsOverride;
                        if (jsonValue == 0)
                        {
                            // Zero is a valid value for this property
                            TimeBetweenCommands = jsonValue;
                        }
                        else
                        {
                            // Value must be a multiple of the clock interval (25ms). 
                            // If it is not, then it must be set to the next value possible
                            // Example 251 would be translated to 275.
                            TimeBetweenCommands = (uint)Math.Ceiling(
                                jsonValue / (double)DriverClock.Clock25ms) * DriverClock.Clock25ms;
                        }
                    }

                    WaitForResponse = DriverData.CrestronSerialDeviceApi.Api.Communication.WaitForResponse;
                    if (DriverData.CrestronSerialDeviceApi.Api.Feedback != null)
                    {
                        SupportsUnsolicitedFeedback = DriverData.CrestronSerialDeviceApi.Api.Feedback.SupportsUnsolicitedFeedback;
                    }
                    PollingEnabled = EnableAutoPolling;

                    Transport.TimeOut = TimeOut;
                    Transport.DriverID = DriverID;

                    if (_baseDeviceProtocolInitialized.Exists())
                    {
                        _baseDeviceProtocolInitialized();
                    }

                    if (DriverData.CrestronSerialDeviceApi.DeviceSupport.Exists())
                    {
                        if (DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(CommonFeatureSupport.SupportsPowerFeedback) &&
                            DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsPowerFeedback])
                        {
                            SupportsPowerFeedback = true;
                        }
                        else if (DriverData.CrestronSerialDeviceApi.DeviceSupport2.Exists() &&
                                 DriverData.CrestronSerialDeviceApi.DeviceSupport2.Contains(CommonFeatureSupport.SupportsPowerFeedback.ToString()))
                        {
                            SupportsPowerFeedback = true;
                        }
                    }

                    UnsuscribeFrom25msClock();
                    SuscribeTo25msClock();
                }
                catch (Exception e)
                {
                    if (EnableLogging)
                    {
                        Log(String.Format("LoadDriver: Problem initializing driver. Reason={0}", e.Message));
                    }

                    if (EnableStackTrace)
                    {
                        Log(String.Format("LoadDriver: {0}", e.StackTrace));
                    }
                }
            }
            else if (EnableLogging)
            {
                Log(String.Format("LoadDriver: DriverData is missing or is the wrong type"));
            }
        }

        /// <summary>
        /// Implementation of IDisposable.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                UnsuscribeFrom25msClock();
                _responseQueueTimer.TryDispose();

                if (Transport.Exists())
                {
                    Transport.DataHandler = null;
                    Transport.MessageTimedOut = null;
                    Transport.ConnectionChanged = null;
                }
                ValidateResponse = null;

                _disposed = true;
            }
        }

        private void SuscribeTo25msClock()
        {
            DriverClock.Driver25msClockEventHandler += InternalDriver25msClock;
        }

        private void UnsuscribeFrom25msClock()
        {
            DriverClock.Driver25msClockEventHandler -= InternalDriver25msClock;
        }

        /// <summary>
        /// This is responsible for all sending logic
        /// </summary>
        private void InternalDriver25msClock()
        {
            if (Math.Abs(CrestronEnvironment.TickCount - _lastPollTick) > PollingInterval)
            {
                _lastPollTick = CrestronEnvironment.TickCount;
                Poll();

                // The driver needs to poll if it supports unsolicited feedback to figure out the connection state.
                // It only needs to do this if polling was disabled.
                if (SupportsUnsolicitedFeedback &&
                    !PollingEnabled &&
                    !string.IsNullOrEmpty(DriverData.CrestronSerialDeviceApi.Api.Feedback.ConnectedStatePollCommand))
                {
                    PollForConnectionState();
                }
            }

            // Only process timed out commands and WarmupCooldown once every second
            if (_checkForTimeOutCounter > 0 && _checkForTimeOutCounter % _oneSecondInClockTicks == 0)
            {
                _checkForTimeOutCounter = 0;
                ProcessUnknownResponses();
                ProcessWarmUpCoolDown();
            }
            else
            {
                _checkForTimeOutCounter++;
            }

            ProcessCommandQueue();

            if (SupportsUnsolicitedFeedback == false)
            {
                // Only update the polling sequence if the driver doesn't support unsolicited feedback
                // UpdatePollingSequence will repopulate the empty polling queue, so it should not be called
                UpdatePollingSequence();
            }
        }

        /// <summary>
        /// Replaces the CTimer instances the framework use to use for warmup/cooldown
        /// by using the driver clock. This will invoke the callbacks <see cref="WarmUpFinished"/> and
        /// <see cref="CoolDownFinished"/> when the driver finishes warmup/cooldown. 
        /// <para>Each device-type library has to assign their own respective Warmup/Cooldown callbacks from
        /// their timers to these methods.</para>
        /// </summary>
        private void ProcessWarmUpCoolDown()
        {
            // Responses to PowerPoll will set _waitingForPowerFeedback to false (it would never be true for a driver with no
            // power feedback) when it either receive a power response, or when it times outs at least two times
            if (WarmingUp == true &&
                Math.Abs(CrestronEnvironment.TickCount - WarmupStartTick) >= WarmUpTime * 1000 &&
                WarmUpFinished != null)
            {
                WarmUpFinished(WarmupStartTick);
                WarmingUp = false;
            }
            else if (CoolingDown == true &&
                    Math.Abs(CrestronEnvironment.TickCount - CooldownStartTick) >= CoolDownTime * 1000 &&
                    CoolDownFinished != null)
            {
                CoolDownFinished(CooldownStartTick);
                CoolingDown = false;
            }
            WarmUpCoolDownProcessed();
        }

        /// <summary>
        /// This should be overridden if the device type protocol wants perform any extra actions
        /// when the framework processes the warmup / cooldown period to see if they are finished.
        /// <para>This is invoked after <see cref="ProcessWarmUpCoolDown"/> is done.</para>
        /// </summary>
        protected virtual void WarmUpCoolDownProcessed()
        { }


        /// <summary>
        /// Determines if the driver has received any data, then
        /// sends the command specified by <see cref="Feedback.ConnectedStatePollCommand"\> 
        /// to test the connection state.
        /// <para>COM drivers that set <see cref="Feedback.SupportsUnsolicitedFeedback"\> to true must provide a 
        /// valid command for <see cref="Feedback.ConnectedStatePollCommand"\>.</para>
        /// <para><see cref="MessageTimedOut"\> will be invoked if the last poll command did not illicit a response
        /// from the device.</para>
        /// </summary>
        private void PollForConnectionState()
        {
            // Check if we have received any response to anything
            if (_connectionPollCommandReceivedResponse == false)
            {
                MessageTimedOut("Feedback.ConnectedStatePollCommand command timed out");
            }

            // Send the command that will illicit a response from the device.
            // It won't matter what it responds with, this is just checking if there
            // is any communication with the device.
            SendCommand(new CommandSet(
                "ConnectedStatePollCommand",
                DriverData.CrestronSerialDeviceApi.Api.Feedback.ConnectedStatePollCommand,
                CommonCommandGroupType.Connection,
                ConnectionStatePollCommandSentCallback,
                false,
                CommandPriority.Special,
                StandardCommandsEnum.NotAStandardCommand));
        }

        /// <summary>
        /// Invoked when the command instantiated by <see cref="PollForConnectionState"\> is sent to the transport.
        /// </summary>
        private void ConnectionStatePollCommandSentCallback()
        {
            // We expect some sort of RX data when we send this command
            // This would only be invoked if PollForConnectionState is invoked, which can only happen when
            // SupportsUnsolicitedFeedback is true.
            _connectionPollCommandReceivedResponse = false;
        }

        /// <summary>
        /// This will check if enough responses came back without being validated by the driver.
        /// The response buffer will clear when enough unrecognized responses come in.
        /// </summary>
        private void ProcessUnknownResponses()
        {
            if (PartialOrUnrecognizedCommand)
            {
                if (PartialOrUnrecognizedCommandCount >= TimeOut / 1000)
                {
                    if (EnableLogging)
                    {
                        Log("Clearing RX buffer - More than 3 responses came in that were not validated by the driver");
                    }
                    ClearRxBuffer();
                }
                else
                {
                    PartialOrUnrecognizedCommandCount++;
                }
            }
        }

        private void ProcessReceivedMessages(object notUsed)
        {
            for (; ; )
            {
                var message = string.Empty;
                message = _receivedMessages.Dequeue(_receivedMessagesQueueTimeout);
                if (_disposed)
                {
                    _receivedMessages.TryDispose();
                    return;
                }

                if (!string.IsNullOrEmpty(message))
                {
                    // TODO: 
                    // Also handle AVF use of SerialTransport
                    if (Transport is SimplTransport)
                    {
                        // Force byte-by-byte when using SimplTransport
                        for (int i = 0; i < message.Length; i++)
                        {
                            ProcessMessage(Convert.ToString(message[i]));
                        }
                    }
                    else
                    {
                        ProcessMessage(message);
                    }
                }
            }
        }

        private void ProcessMessage(string message)
        {
            if (_rxOut != null)
            {
                _rxOut(message);
            }

            message.AppendToStringBuffer(RxData, _rxDataLock);
            ValidatedRxData validatedData = null;
            try
            {
                var rxBufferToDriver = RxData.ToString(_rxDataLock);
                // TODO: If we introduce logging levels, this needs to be verbose
                if (EnableStackTrace)
                {
                    Log(string.Format("Sending RX buffer to ValidateResonse: {0}", BitConverter.ToString(Encoding.GetBytes(rxBufferToDriver))));
                }

                validatedData = ValidateResponse(rxBufferToDriver, LastCommandGroup);
            }
            catch (Exception e)
            {
                validatedData = new ValidatedRxData(false, string.Empty);
                validatedData.Ignore = true;
                if (EnableLogging)
                {
                    Log(string.Format("Exception encountered while validating response: {0}", e.ToString()));
                }
            }

            // Check if we received data while we were disconnected
            if (!IsConnected)
            {
                IsConnected = true;
                ConnectionChanged(true);
            }

            // Check if we should ignore all the data we have received so far
            if (validatedData.Ignore)
            {
                if (EnableLogging)
                {
                    Log("Clearing RX buffer - ValidatedData marked Ignore as true");
                }
                ClearRxBuffer();
                return;
            }

            // Check if data is ready
            if (!validatedData.Ready || string.IsNullOrEmpty(validatedData.Data))
            {
                PartialOrUnrecognizedCommand = true;
                return;
            }

            // Full packet has been received
            if (EnableLogging)
            {
                Log("Clearing RX buffer - ValidatedData marked Ready as true");
            }
            ClearRxBuffer();

            if (validatedData.CommandGroup != CommonCommandGroupType.Unknown)
            {
                LastCommandGroup = validatedData.CommandGroup;
            }
            else if (WaitForResponse && validatedData.CommandGroup == CommonCommandGroupType.Unknown)
            {
                validatedData.CommandGroup = LastCommandGroup;
            }

            AcknowledgeValidatedFeedback(validatedData.CommandGroup, validatedData.CustomCommandGroup);
            ChooseDeconstructMethod(validatedData);
        }

        /// <summary>
        /// Invoked by the transport class when it receives data from the controlled device.
        /// The default implementation of this is it adds the response to an instance of CrestronQueue
        /// that another thread monitors and eventually calls ValidateResponse with.
        /// </summary>
        /// <param name="rx">The data received from the device.</param>
        public virtual void DataHandler(string rx)
        {
            // This will be called on a seperate thread.
            // Queue the received message and let the main thread handle processing so this thread does not stay alive for too long
            try
            {
                var enqueued = _receivedMessages.Enqueue(rx, 100);
                if (!enqueued &&
                    EnableLogging)
                {
                    Log("Unable to enqueue received message into receive buffer");
                }
                // Keep track of the last time we received data from the device
                _connectionPollCommandReceivedResponse = true;
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Log(string.Format("Exception occured in DataHandler - {0}", e.Message));
                }
            }
        }

        /// <summary>
        /// Logs a message to console or CustomLogger with a prepended header.
        /// </summary>
        /// <param name="message">The message to log</param>
        protected void Log(string message)
        {
            if (EnableLogging)
            {
                message = string.Format("{0}::{1}::({2}) {3} : {4}",
                        CrestronEnvironment.TickCount,
                        DateTime.Now,
                        DriverID,
                        ProtocolName,
                        message);


                if (CustomLogger == null)
                {
                    CrestronConsole.PrintLine(message);
                }
                else
                {
                    CustomLogger(message);
                }
            }
        }

        /// <summary>
        /// Log helper to log a command that is not supported by the driver.
        /// </summary>
        /// <param name="commandName">That command name that is not supported</param>
        protected void LogCommandNotSupported(string commandName)
        {
            if (EnableLogging)
            {
                var logStatement = String.Format("({0}) {1} : The command {2} is not supported", DriverID, ProtocolName,
                    commandName);
                Log(logStatement);
            }
        }

        /// <summary>
        /// This is invoked whenever the connection state changes, either via the transport class
        /// or there being too many time-outs in this class.
        /// <para>The base method must be called if this is overridden unless the driver sees a need for hiding connected/disconnected feedback,
        /// such as in cases where it can be predicted. Example: Device shuts off LAN port for a few seconds when it is powered on initially.</para>
        /// <para>For drivers that set SupportsUnsolicitedFeedback to true, this will restart the polling sequence if the connection state when from false to true</para>
        /// </summary>
        /// <param name="connection">The new connected state</param>
        protected virtual void ConnectionChanged(bool connection)
        {
            if (EnableLogging)
            {
                Log(string.Format("Connection state changed - Current state: {0} - New state: {1}", IsConnected, connection));
            }

            if (SupportsUnsolicitedFeedback &&
                !IsConnected &&
                connection)
            {
                // We were disconnected, now we are connected and we support unsolicited feedback
                // Re-enable polling
                PollingEnabled = true;

                if (EnableLogging)
                {
                    Log("Unsolicited feedback is supported and the driver went from a disconnected to connected state. Resuming polling now.");
                }
            }

            IsConnected = connection;
            SendCommands = connection;
            PendingRequest = null;

            if (!connection)
            {
                TimeDisconnected = DateTime.Now;
            }

            ConnectionChangedEvent(connection);
        }

        /// <summary>
        /// Invoked when a poll command that was recently sent has not received a response
        /// in at least ResponeTimeOut ms. If there are five or more timeouts, this will 
        /// invoked ConnectionChangedEvent with a disconnected state if the transport.IsEthernetTransport
        /// is set to false.
        /// </summary>
        /// <param name="timeoutMessage">The message to log that explains what timed out</param>
        protected virtual void MessageTimedOut(string timeoutMessage)
        {
            if (EnableLogging)
            {
                Log(string.Format("MessageTimedOut: {0}", timeoutMessage));
            }
            if (TimeoutCount >= 5)
            {
                if (!(Transport is ATransportDriver) || (Transport is ATransportDriver && !(Transport as ATransportDriver).IsEthernetTransport))
                {
                    IsConnected = false;
                    ConnectionChangedEvent(false);
                }
            }

            TimeoutCount++;
        }

        #region ICustomCommandCollection
        public virtual void AddCustomCommand(string commandName, string commandValue, List<Parameters> parameters)
        {
            if (string.IsNullOrEmpty(commandName) || string.IsNullOrEmpty(commandValue))
                return;

            if (parameters == null)
            {
                parameters = new List<Parameters>();
            }
            if (CheckIfCustomCommandExists(commandName) == false &&
                DriverData.CrestronSerialDeviceApi.Api.CustomCommands.Exists())
            {
                DriverData.CrestronSerialDeviceApi.Api
                    .CustomCommands.Add(new CustomCommand
                    {
                        Command = commandValue,
                        Name = commandName,
                        Parameters = parameters
                    });
            }
        }

        public virtual bool CheckIfCustomCommandExists(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                return false;

            return DriverData.CrestronSerialDeviceApi.Api.CustomCommands != null
                && DriverData.CrestronSerialDeviceApi.Api.CustomCommands.Count > 0
                && DriverData.CrestronSerialDeviceApi.Api.CustomCommands
                       .Any(cmd => (cmd.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)));


        }

        public virtual List<string> CustomCommandNames
        {
            get
            {
                if ((DriverData.CrestronSerialDeviceApi.Api
                        .CustomCommands == null) ||
                        (DriverData.CrestronSerialDeviceApi.Api
                        .CustomCommands.Count == 0))
                    return new List<string>();

                return (DriverData.CrestronSerialDeviceApi.Api
                    .CustomCommands.Select(cmd => cmd.Name).ToList());
            }
        }

        public virtual bool RemoveCustomCommandByName(string commandName)
        {
            if (CheckIfCustomCommandExists(commandName) == false)
                return false;

            try
            {
                DriverData.CrestronSerialDeviceApi.Api
                        .CustomCommands.Remove
                        (DriverData.CrestronSerialDeviceApi.Api
                            .CustomCommands.First(cmd => (cmd.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase))));
            }
            catch { return false; }
            return true;
        }

        public virtual void SendCustomCommandByName(string commandName)
        {
            if (CheckIfCustomCommandExists(commandName))
            {
                var foundCommand =
                    DriverData.CrestronSerialDeviceApi.Api
                    .CustomCommands.FirstOrDefault(cmd => (cmd.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)));

                if (foundCommand != null)
                {
                    SendCommand(
                        new CommandSet(foundCommand.Name, foundCommand.Command, CommonCommandGroupType.Unknown,
                            null, false, CommandPriority.Normal, StandardCommandsEnum.CustomCommand));
                }
                else
                {
                    LogCommandNotSupported(commandName);
                }
            }
        }

        public virtual void SendCustomCommandValue(string commandValue)
        {
            Transport.Send(commandValue, null);
        }

        #endregion

        #region ICustomCommandCollection2

        public virtual void SendCustomCommandByName(string commandName, CommandAction action)
        {
            switch (action)
            {
                case CommandAction.None:
                    SendCustomCommandByName(commandName);
                    break;
                case CommandAction.Hold:
                    PressCustomCommand(commandName);
                    break;
                case CommandAction.Release:
                    ReleaseCustomCommand(commandName);
                    break;
            }
        }

        protected virtual void PressCustomCommand(string commandName)
        {
            if (_customCommandRampTimer == null)
            {
                _customCommandRampTimer = new CTimer(CustomCommandRampTick, commandName, 0, 250);
            }
            _customCommandIsRamping = true;
        }

        protected virtual void ReleaseCustomCommand(string commandName)
        {
            if (_customCommandRampTimer == null)
            {
                return;
            }
            _customCommandRampTimer.TryDispose();

            if (_customCommandIsRamping &&
                _customCommandRampTicks == 0)
            {
                SendCustomCommandByName(commandName);
            }

            _customCommandRampTicks = 0;
            _customCommandIsRamping = false;
        }

        protected virtual void CustomCommandRampTick(object commandName)
        {
            if (!DriverLoaded ||
                !(commandName is string))
            {
                return;
            }
            if (_customCommandIsRamping)
            {
                _customCommandRampTicks++;
                SendCustomCommandByName(commandName as string);
            }
        }

        #endregion ICustomCommandCollection2

        private void ClearRxBuffer()
        {
            _checkForTimeOutCounter = 0;
            PartialOrUnrecognizedCommand = false;
            PartialOrUnrecognizedCommandCount = 0;
            RxData.Clear(_rxDataLock);
            TimeoutCount = 0;
            PendingRequest = null;
        }

        #region User Attributes

        /// <summary>
        /// Drivers should override this to capture any attribute values they may need. 
        /// The driver is responsible for validating the ID and using the value.
        /// </summary>
        public virtual void SetUserAttribute(string attributeId, string attributeValue)
        {
            Log("SetUserAttribute not implemented by the driver");
        }

        /// <summary>
        /// Drivers should override this to capture any attribute values they may need. 
        /// The driver is responsible for validating the ID and using the value.
        /// </summary>
        public virtual void SetUserAttribute(string attributeId, bool attributeValue)
        {
            Log("SetUserAttribute not implemented by the driver");
        }

        /// <summary>
        /// Drivers should override this to capture any attribute values they may need. 
        /// The driver is responsible for validating the ID and using the value.
        /// </summary>
        public virtual void SetUserAttribute(string attributeId, ushort attributeValue)
        {
            Log("SetUserAttribute not implemented by the driver");
        }

        #endregion User Attributes

        /// <summary>
        /// This is invoked by <see cref="ConnectionChanged"/> and every device type needs
        /// to implement this to throw their appropriate connection event.
        /// <para>Drivers may not override this and should use <see cref="ConnectionChanged"/> if they need the new connection state.</para>
        /// </summary>
        /// <param name="connection">The new connected state</param>
        protected abstract void ConnectionChangedEvent(bool connection);

        /// <summary>
        /// Invoked after a driver returns an instance of ValidatedRxData that has Ready set to true.
        /// The default implementation will switch based on the command group of the object and 
        /// </summary>
        /// <param name="validatedData">The response from a device that is ready to be parsed</param>
        protected abstract void ChooseDeconstructMethod(ValidatedRxData validatedData);

        /// <summary>
        /// This method will be called every PollingInterval ms.  
        /// Any non-standard polling should be handled here. The base method has no logic.
        /// </summary>
        protected virtual void Poll()
        { }

        /// <summary>
        /// This will update the persistent polling commands in the queue if the driver has changed the polling sequence at run-time.
        /// Only updating PowerOnPollingSequence since no commands except Power group commands are sent while the driver believes the device to be off.
        /// <para>This will not be called internally if the driver supports unsolicited feedaback. It will only be called when the connection state changes from false to true</para>
        /// </summary>
        protected virtual void UpdatePollingSequence()
        { }
    }
}