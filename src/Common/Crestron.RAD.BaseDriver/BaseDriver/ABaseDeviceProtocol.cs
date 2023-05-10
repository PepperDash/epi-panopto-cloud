using System;
using System.Linq;
using System.Text;
using Crestron.RAD.Common;
using Crestron.RAD.Common.ExtensionMethods;
using Crestron.SimplSharp;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.Common.Enums;
using System.Collections.Generic;

namespace Crestron.RAD.BaseDriver
{
    public delegate void RxOut(string message);
    public delegate ValidatedRxData ResponseValidator(string response, CommonCommandGroupType commandGroup);

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
        private bool _disposed = false;
        private const int _maxQueueSize = 50;
        private const int _tickRate = 1000;
        private readonly CTimer _tick;
        private const int TickRate = 1000;
        protected static StringBuilder RxData;
        protected readonly CCriticalSection CriticalSection = new CCriticalSection();
        protected BaseRootObject DriverData;
        public string ProtocolName { get; protected set; }
        public Action<string> CustomLogger { get; set; }
        public event RxOut RxOut;
        public bool DriverLoaded { get; protected set; }
        public bool EnableLogging { get; set; }
        public bool EnableStackTrace { get; set; }
        public bool LogTxAndRxAsBytes { get; set; }
        public bool IsConnected { get; protected set; }
        public bool PowerIsOn { get; protected set; }
        public bool WarmingUp { get; protected set; }
        public bool CoolingDown { get; protected set; }
        protected CommandSet PendingRequest;
        protected ResponseValidator ValidateResponse { get; set; }
        public DateTime TimeDisconnected;
        protected ATransportDriver Transport;
        protected Encoding Encoding { get; set; }
        protected MinHeap<CommandSet> CommandQueue;
        protected uint TimeOut;
        protected int TimeoutCount;
        protected uint TimeBetweenCommands;
        protected int TickCounter;
        protected int TicksBetweenPolls;
        protected int InternalPollingInterval { get; set; }
        protected bool SendCommands;
        protected bool EnableAutoPolling;
        protected bool PartialOrUnrecognizedCommand;
        protected int PartialOrUnrecognizedCommandCount;
        protected bool PollingEnabled;
        protected bool WaitForResponse;
        protected bool QueueCommands;
        protected int MinutesUntilRemoved;
        protected CommonCommandGroupType LastCommandGroup;
        protected CTimer RemoveFromQueue;
        protected CTimer WaitTimer;
        protected int RemoveFromQueueRepeat = 120000;
        protected byte Id { set; get; }
        public int DriverID;

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
                TicksBetweenPolls = (newPollingInterval / TickRate) - 1;
            }
            get { return InternalPollingInterval; }
        }

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
            PollingInterval = 4000;
            Encoding = Encoding.GetEncoding("ISO-8859-1");

            _tick = new CTimer(Tick, null, _tickRate, _tickRate);
            WaitTimer = new CTimer(WaitOver, Timeout.Infinite);
            RemoveFromQueue = new CTimer(CheckQueue, Timeout.Infinite);
            CommandQueue = new MinHeap<CommandSet>(_maxQueueSize);
            RxData = new StringBuilder();

            PowerIsOn = false;
            WarmingUp = false;
            CoolingDown = false;
            IsConnected = false;
        }

        public virtual void Initialize(object driverData)
        {
            if (driverData != null && driverData.GetType() == typeof(BaseRootObject))
            {
                DriverData = (BaseRootObject)driverData;
                try
                {

                    TimeOut = DriverData.CrestronSerialDeviceApi.Api.Communication.ResponseTimeout;
                    EnableAutoPolling = DriverData.CrestronSerialDeviceApi.Api.Communication.EnableAutoPolling;
                    TimeBetweenCommands = DriverData.CrestronSerialDeviceApi.Api.Communication.TimeBetweenCommands;
                    WaitForResponse = DriverData.CrestronSerialDeviceApi.Api.Communication.WaitForResponse;
                    PollingEnabled = EnableAutoPolling;

                    Transport.TimeOut = TimeOut;
                    Transport.DriverID = DriverID;
                }
                catch (Exception e)
                {
                    Log(String.Format("LoadDriver: Problem initializing driver. Reason={0}", e.Message));

                    if (EnableStackTrace)
                    {
                        Log(String.Format("LoadDriver: {0}", e.StackTrace));
                    }
                }
            }
            else
            {
                Log(String.Format("LoadDriver: DriverData is missing or is the wrong type"));
            }
        }

        public virtual void Tick(object obj)
        {
            if (!DriverLoaded)
            {
                return;
            }

            if (PartialOrUnrecognizedCommand)
            {
                if (PartialOrUnrecognizedCommandCount >= TimeOut / TickRate)
                {
                    Log(String.Format("{0} not recognized", RxData));
                    RxData.Length = 0;
                    RxData.Capacity = 0;
                    PendingRequest = null;
                    PartialOrUnrecognizedCommand = false;
                    PartialOrUnrecognizedCommandCount = 0;

                    RemoveCommandFromQueue();
                }

                PartialOrUnrecognizedCommandCount++;
            }
            if (TickCounter == 0 && PollingEnabled)
            {
                TickCounter++;
                Poll();
            }
            else if (TickCounter == TicksBetweenPolls)
            {
                TickCounter = 0;
            }
            else
            {
                TickCounter++;
            }
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _tick.TryDispose();
                _disposed = true;
            }
        }

        public virtual void DataHandler(string rx)
        {
            if (RxOut != null)
            {
                RxOut(rx);
            }

            RxData.Append(rx);

            ValidatedRxData validatedData = ValidateResponse(RxData.ToString(), LastCommandGroup);

            // Check if we received data while we were disconnected
            if (!IsConnected)
            {
                IsConnected = true;
                ConnectionChanged(true);
            }

            // Check if we should ignore all the data we have received so far
            if (validatedData.Ignore)
            {
                ClearRxBuffer();

                if (WaitForResponse)
                {
                    RemoveCommandFromQueue();
                }
                return;
            }

            // Check if data is ready
            if (!validatedData.Ready || string.IsNullOrEmpty(validatedData.Data))
            {
                PartialOrUnrecognizedCommand = true;
                return;
            }

            // Full packet has been received
            ClearRxBuffer();

            if (validatedData.CommandGroup != CommonCommandGroupType.Unknown)
            {
                LastCommandGroup = validatedData.CommandGroup;
            }
            else if (WaitForResponse && validatedData.CommandGroup == CommonCommandGroupType.Unknown)
            {
                validatedData.CommandGroup = LastCommandGroup;
            }

            string data = validatedData.Data;

            ChooseDeconstructMethod(validatedData);

            if (WaitForResponse)
            {
                PendingRequest = null;
                RemoveCommandFromQueue();
            }
        }

        protected void RemoveCommandFromQueue()
        {
            CommandSet command = null;

            try
            {
                CriticalSection.Enter();

                if (CommandQueue.Count != 0)
                {
                    command = CommandQueue.Extract();
                }
            }
            catch (Exception e)
            {
                Log(String.Format("RemoveCommandFromQueue failure. Reason={0}", e.Message));

                if (EnableStackTrace)
                {
                    Log(String.Format("RemoveCommandFromQueue {0}", e.StackTrace));
                }
            }
            finally
            {
                CriticalSection.Leave();
            }

            if (command != null)
            {
                SendToTransport(command);
            }
        }

        protected void Log(string message)
        {
            if (EnableLogging)
            {
                if (CustomLogger == null)
                {
                    CrestronConsole.PrintLine(String.Format("({0}) {1} : {2}", DriverID, ProtocolName, message));
                }
                else
                {
                    CustomLogger(String.Format("({0}) {1} : {2}\n", DriverID, ProtocolName, message));
                }
            }
        }

        protected void LogCommandNotSupported(string commandName)
        {
            var logStatement = String.Format("({0}) {1} : The command {2} is not supported", DriverID, ProtocolName,
                commandName);
            Log(logStatement);
        }

        protected virtual void ConnectionChanged(bool connection)
        {
            IsConnected = connection;
            SendCommands = connection;

            PendingRequest = null;
            if (connection)
            {
                RemoveFromQueue.Stop();
                try
                {
                    CriticalSection.Enter();

                    if (CommandQueue.Count != 0)
                    {
                        if (PowerIsOn || (!PowerIsOn && CommandQueue.Values[0].CommandGroup == CommonCommandGroupType.Power))
                        {
                            var command = CommandQueue.Extract();
                            PrepareStringThenSend(command);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(String.Format("Tick (CriticalSection) failure. Reason={0}", e.Message));

                    if (EnableStackTrace)
                    {
                        Log(String.Format("Tick (CriticalSection): {0}", e.StackTrace));
                    }
                }
                finally
                {
                    CriticalSection.Leave();
                }

            }
            else
            {
                TimeDisconnected = DateTime.Now;
                RemoveFromQueue.Reset(RemoveFromQueueRepeat, RemoveFromQueueRepeat);
            }
            ConnectionChangedEvent(connection);
        }

        protected virtual void MessageTimedOut(string lastSentCommand)
        {
            if (EnableLogging)
            {
                byte[] buf = Encoding.GetBytes(lastSentCommand);

                StringBuilder debugStringBuilder = new StringBuilder();
                debugStringBuilder.Append(" : MessageTimedOut: ");
                debugStringBuilder.Append(LogTxAndRxAsBytes ? BitConverter.ToString(buf).Replace("-", " ") : lastSentCommand);
                Log(debugStringBuilder.ToString());
            }

            if (PendingRequest != null && TimeoutCount == 0) //resend command if this is the first timeout
            {
                var resend = PendingRequest;
                PendingRequest = null;
                Transport.Send(resend.Command, resend.Parameters);

                if (resend.CallBack != null)
                {
                    resend.CallBack();
                }
            }
            else
            {
                PendingRequest = null;
                RemoveCommandFromQueue();
            }

            if (TimeoutCount >= 5 && !Transport.IsEthernetTransport)
            {
                IsConnected = false;
                ConnectionChangedEvent(false);
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
            if (CheckIfCustomCommandExists(commandName) == false)
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
                       .Any(cmd => cmd.Name.ToLower().Equals(commandName.ToLower()));
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
                            .CustomCommands.First(cmd => cmd.Name.ToLower() == commandName.ToLower()));
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
                    .CustomCommands.FirstOrDefault(cmd => cmd.Name.ToLower() == commandName.ToLower());

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

        private void CheckQueue(object obj)
        {
            if ((DateTime.Now - TimeDisconnected).TotalMinutes >= MinutesUntilRemoved)
            {
                CommandQueue.Clear();
            }
        }

        private void ClearRxBuffer()
        {
            PartialOrUnrecognizedCommand = false;
            PartialOrUnrecognizedCommandCount = 0;

            RxData.Length = 0;
            RxData.Capacity = 0;
            TimeoutCount = 0;
        }

        internal void WaitOver(object userspecific)
        {
            PendingRequest = null;
            RemoveCommandFromQueue();
        }

        protected abstract void ConnectionChangedEvent(bool connection);
        protected abstract void ChooseDeconstructMethod(ValidatedRxData validatedData);
        protected abstract void Poll();
    }
}