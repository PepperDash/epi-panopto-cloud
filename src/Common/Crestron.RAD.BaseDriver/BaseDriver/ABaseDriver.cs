// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.
using System;
using System.Linq;
using Crestron.RAD.Common;
using Crestron.RAD.Common.ExtensionMethods;
using Crestron.RAD.Common.Transports;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharp.CrestronIO;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace Crestron.RAD.BaseDriver
{
    public abstract class ABasicDriver : IBasicInformation, IConnection, IBasicLogger, IDisposable
    {
        private bool _disposed = false;
        private IAuthentication _authentication;

        private ABaseDriverProtocol _deviceProtocol;
        protected ABaseDriverProtocol DeviceProtocol
        {
            get { return _deviceProtocol; }
            set
            {
                _deviceProtocol = value;
                if (_deviceProtocol != null)
                {
                    _deviceProtocol.DriverID = DriverID;
                }
            }
        }

        protected ISerialTransport ConnectionTransport;
        protected internal BaseRootObject DriverData;
        protected static string DeviceTypeName;
        protected string JsonString;

        protected int DriverID;
        private static int _driversLoaded = 0;
        private static bool _consoleCommandsLoaded;
        private delegate void DriverConsoleCommand(ConsoleCommandType command, string args);
        private static event DriverConsoleCommand DriverConsoleCommandEvent;
        private enum ConsoleCommandType { RADInfo = 1, TxDebug = 2, RxDebug = 3, StackTrace = 4, General = 5 }

        public abstract CType AbstractClassType { get; }
        public abstract void ConvertJsonFileToDriverData(string jsonString);

        public byte Id { get; set; }
        public int Port { get { return DriverData.CrestronSerialDeviceApi.Api.Communication.Port; } }
        protected bool EnableAutoReconnect { get { return DriverData.CrestronSerialDeviceApi.Api.Communication.EnableAutoReconnect; } }
        public ComPortSpec ComSpec
        {
            get
            {
                return new ComPortSpec
                {
                    BaudRate = DriverData.CrestronSerialDeviceApi.Api.Communication.Baud,
                    DataBits = DriverData.CrestronSerialDeviceApi.Api.Communication.DataBits,
                    HardwareHandShake = DriverData.CrestronSerialDeviceApi.Api.Communication.HwHandshake,
                    Parity = DriverData.CrestronSerialDeviceApi.Api.Communication.Parity,
                    Protocol = DriverData.CrestronSerialDeviceApi.Api.Communication.Protocol,
                    StopBits = DriverData.CrestronSerialDeviceApi.Api.Communication.StopBits,
                    SoftwareHandshake = DriverData.CrestronSerialDeviceApi.Api.Communication.SwHandshake
                };
            }
        }

        public ABasicDriver()
        {
            LoadDriverData();

            if (!_consoleCommandsLoaded && CrestronEnvironment.RuntimeEnvironment == eRuntimeEnvironment.SimplSharpPro)
            {
                CrestronConsole.AddNewConsoleCommand(ConsoleLoggingToggle, "RADLogging", "Toggles general logging for the driver", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleTxDebugToggle, "RADTxDebug", "Toggles connection transport (TX) logging", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleRxDebugToggle, "RADRxDebug", "Toggles connection transport (RX) logging", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleStackTraceToggle, "RADStackTrace", "Toggles stack trace printing when exceptions occur", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleRadInfo, "RADInfo", "Prints out information regarding all the loaded drivers", ConsoleAccessLevelEnum.AccessProgrammer);
                _consoleCommandsLoaded = true;
            }

            DriverID = ++_driversLoaded;
            DriverConsoleCommandEvent += new DriverConsoleCommand(DriverConsoleCommandCallback);
            Connected = false;
        }


        public void Initialize(BaseRootObject driverData)
        {
            DriverData = driverData;
            CrestronDataStoreWrapper dataStoreWrapper = new CrestronDataStoreWrapper();
            dataStoreWrapper.Initialize();
            _authentication = new Authentication(DriverData.CrestronSerialDeviceApi.Api.Communication.Authentication, dataStoreWrapper);
        }

        private void LoadDriverData()
        {
            JsonString = ReadJsonFile();
            ConvertJsonFileToDriverData(JsonString);
        }

        protected JsonSerializerSettings CreateSerializerSettings()
        {
            var authenticationNodeConverter = new AuthenticationJsonConverter();
            var serializerSettings = new JsonSerializerSettings { Converters = { authenticationNodeConverter } };
            return serializerSettings;
        }

        private string ReadJsonFile()
        {
            string dataFile = string.Empty;
            string resourceName = string.Empty;

            Assembly assembly = AbstractClassType.Assembly;
            resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.ToLower().EndsWith(".json"));

            if (!string.IsNullOrEmpty((resourceName)))
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataFile = reader.ReadToEnd();
                        }
                    }
                }
            }
            else
            {
                Log("Driver JSON file is missing");
            }
            return dataFile;
        }

        #region Console Commands

        private void DriverConsoleCommandCallback(ABasicDriver.ConsoleCommandType command, string args)
        {
            switch (command)
            {
                case ConsoleCommandType.General:
                    LoggingToggle(args);
                    break;

                case ConsoleCommandType.RADInfo:
                    ShowRadInfo(args);
                    break;

                case ConsoleCommandType.RxDebug:
                    RxDebugToggle(args);
                    break;

                case ConsoleCommandType.StackTrace:
                    StackTraceToggle(args);
                    break;

                case ConsoleCommandType.TxDebug:
                    TxDebugToggle(args);
                    break;
            }
        }

        private void ConsoleRadInfo(string args)
        {
            DriverConsoleCommandEvent(ConsoleCommandType.RADInfo, args);
        }

        private void ConsoleLoggingToggle(string args)
        {
            DriverConsoleCommandEvent(ConsoleCommandType.General, args);
        }

        private void ConsoleTxDebugToggle(string args)
        {
            DriverConsoleCommandEvent(ConsoleCommandType.TxDebug, args);
        }

        private void ConsoleRxDebugToggle(string args)
        {
            DriverConsoleCommandEvent(ConsoleCommandType.RxDebug, args);
        }

        private void ConsoleStackTraceToggle(string args)
        {
            DriverConsoleCommandEvent(ConsoleCommandType.StackTrace, args);
        }

        private void ShowRadInfo(string args)
        {
            CrestronConsole.PrintLine("\u000D--- RAD Info for Driver ID {0}---", DriverID);
            CrestronConsole.PrintLine("Driver: {0} - {1}", Manufacturer, BaseModel);
            CrestronConsole.PrintLine("Type: {0}", DriverData.CrestronSerialDeviceApi.GeneralInformation.DeviceType);
            CrestronConsole.PrintLine("GUID: {0}", Guid);
            CrestronConsole.PrintLine("SDK Version: {0}", DriverData.CrestronSerialDeviceApi.GeneralInformation.SdkVersion);
            CrestronConsole.PrintLine("Version {0} ({1})", DriverVersion, VersionDate.Date);
            CrestronConsole.PrintLine("--- RAD Info End ---\u000D");
        }

        private void LoggingToggle(string args)
        {
            if (IsValidCommandParameter(args) && IsValidDriverID(args))
            {
                EnableLogging = GetStateFromCommandParameter(args);
            }
        }

        private void TxDebugToggle(string args)
        {
            if (IsValidCommandParameter(args) && IsValidDriverID(args))
            {
                EnableTxDebug = GetStateFromCommandParameter(args);
            }
        }

        private void RxDebugToggle(string args)
        {
            if (IsValidCommandParameter(args) && IsValidDriverID(args))
            {
                EnableRxDebug = GetStateFromCommandParameter(args);
            }
        }

        private void StackTraceToggle(string args)
        {
            if (IsValidCommandParameter(args) && IsValidDriverID(args))
            {
                EnableStackTrace = GetStateFromCommandParameter(args);
            }
        }

        private bool IsValidDriverID(string args)
        {
            var idValue = args.Split(' ')[0];

            try
            {
                var id = int.Parse(idValue);

                if (id == DriverID || id == 0)
                {
                    return true;
                }
            }
            catch
            { }

            return false;
        }

        private bool GetStateFromCommandParameter(string args)
        {
            var state = args.Split(' ')[1].ToLower();

            if (state == "on")
            {
                return true;
            }

            return false;
        }

        private bool IsValidCommandParameter(string args)
        {
            args = args.ToLower();

            if (args.Contains(' '))
            {
                var arguments = args.Split(' ');

                if (arguments[1] == "on" || arguments[1] == "off")
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IBasicInformation Members

        public Guid Guid
        {
            get
            { return DriverData.CrestronSerialDeviceApi.GeneralInformation.Guid; }
        }

        public string Description
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.Description; }
        }

        public string Manufacturer
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.Manufacturer; }
        }

        public string BaseModel
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.BaseModel; }
        }

        public string DriverVersion
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.DriverVersion; }
        }

        public List<string> SupportedSeries
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.SupportedSeries; }
        }

        public List<string> SupportedModels
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.SupportedModels; }
        }

        public DateTime VersionDate
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.VersionDate; }
        }

        public bool SupportsFeedback
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(CommonFeatureSupport.SupportsFeedback)
                        && DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsFeedback];
            }
        }

        #endregion

        #region Basic Connection

        public bool Connected { get; protected set; }

        public virtual bool SupportsDisconnect
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(CommonFeatureSupport.SupportsDisconnect)
                    && DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsDisconnect];
            }
        }
        public virtual void Disconnect()
        {
            if (ConnectionTransport != null)
            {
                ConnectionTransport.Stop();
            }
        }

        public virtual bool SupportsReconnect
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(CommonFeatureSupport.SupportsReconnect)
                    && DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsReconnect];
            }
        }
        public virtual void Reconnect()
        {
            if (ConnectionTransport != null)
            {
                if (ConnectionTransport.IsConnected)
                {
                    ConnectionTransport.Stop();
                }
                ConnectionTransport.Start();
            }
        }

        public virtual void Connect()
        {
            if (ConnectionTransport != null)
            {
                ConnectionTransport.Start();
            }
        }

        #endregion Basic Connection

        #region Custom Command

        /// <summary>
        /// Method to send a custom command.
        /// </summary>
        /// <param name="command">Custom command.</param>
        public void SendCustomCommand(string command)
        {
            ConnectionTransport.Send(command, null);
        }

        /// <summary>
        /// Enables RxOut event.
        /// </summary>
        public bool EnableRxOut { get; set; }

        /// <summary>
        /// Sends strings sent from the device.
        /// </summary>
        public event Action<string> RxOut;

        /// <summary>
        /// Method to send the RX Out event if Rx Out is enabled.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void SendRxOut(string message)
        {
            if (RxOut != null && EnableRxOut)
            {
                RxOut(message);
            }
        }

        #endregion Custom Command

        #region ICustomCommandCollection Members

        public virtual void AddCustomCommand(string commandName, string commandValue, List<Parameters> parameters)
        {
            if (DeviceProtocol != null)
            {
                DeviceProtocol.AddCustomCommand(commandName, commandValue, parameters);
            }
        }

        public virtual bool CheckIfCustomCommandExists(string commandName)
        {
            if (DeviceProtocol != null)
            {
                return DeviceProtocol.CheckIfCustomCommandExists(commandName);
            }

            return false;
        }

        public virtual List<string> CustomCommandNames
        {
            get
            {
                if (DeviceProtocol != null)
                {
                    return DeviceProtocol.CustomCommandNames;
                }

                return new List<string>();
            }
        }

        public virtual bool RemoveCustomCommandByName(string commandName)
        {
            if (DeviceProtocol != null)
            {
                return DeviceProtocol.RemoveCustomCommandByName(commandName);
            }

            return false;
        }

        public virtual void SendCustomCommandByName(string commandName)
        {
            if (DeviceProtocol != null)
            {
                DeviceProtocol.SendCustomCommandByName(commandName);
            }
        }

        public virtual void SendCustomCommandValue(string commandValue)
        {
            ConnectionTransport.Send(commandValue, null);
        }

        #endregion

        #region Logging

        protected bool InternalEnableStackTrace;
        public bool EnableStackTrace
        {
            set
            {
                if (DeviceProtocol != null)
                {
                    DeviceProtocol.EnableStackTrace = value;
                }
            }
            get { return InternalEnableStackTrace; }
        }

        protected bool InternalEnableTxDebug;
        public bool EnableTxDebug
        {
            set
            {
                InternalEnableTxDebug = value;
                if (ConnectionTransport != null)
                {
                    ConnectionTransport.EnableTxDebug = value;
                }
            }
            get { return InternalEnableTxDebug; }
        }

        protected bool InternalEnableRxDebug;
        public bool EnableRxDebug
        {
            set
            {
                InternalEnableRxDebug = value;
                if (ConnectionTransport != null)
                {
                    ConnectionTransport.EnableRxDebug = value;
                }
            }
            get { return InternalEnableRxDebug; }
        }

        protected bool InternalEnableLogging;
        public bool EnableLogging
        {
            set
            {
                InternalEnableLogging = value;

                if (ConnectionTransport != null)
                {
                    ConnectionTransport.EnableLogging = value;
                }
                if (DeviceProtocol != null)
                {
                    DeviceProtocol.EnableLogging = value;
                }
            }
            get { return InternalEnableLogging; }
        }

        protected Action<string> InternalCustomLogger;
        public Action<string> CustomLogger
        {
            set
            {
                InternalCustomLogger = value;

                if (ConnectionTransport != null)
                {
                    ConnectionTransport.CustomLogger = value;
                }
                if (DeviceProtocol != null)
                {
                    DeviceProtocol.CustomLogger = value;
                }
            }
            get { return InternalCustomLogger; }
        }

        protected void Log(string message)
        {
            if (!InternalEnableLogging) return;

            if (InternalCustomLogger == null)
            {
                CrestronConsole.PrintLine(message);
            }
            else
            {
                InternalCustomLogger(message + "\n");
            }
        }

        protected void LogCommandNotSupported(string commandName)
        {
            var logStatement = string.Format("{0} : The command {1} is not supported.",
                DeviceTypeName, commandName);
            Log(logStatement);
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                if (ConnectionTransport.Exists())
                {
                    ConnectionTransport.Stop();
                }
                if (DeviceProtocol.Exists())
                {
                    DeviceProtocol.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion

        #region IAuthentication Members

        public virtual bool SupportsUsername
        {
            get { return _authentication.SupportsUsername; }
        }

        public virtual string UsernameMask
        {
            get { return _authentication.UsernameMask; }
        }

        public virtual string UsernameKey
        {
            set { _authentication.UsernameKey = value; }
        }

        public virtual bool SupportsPassword 
        {
            get { return _authentication.SupportsPassword; }
        }

        public virtual string PasswordMask
        {
            get { return _authentication.PasswordMask; }
        }

        public virtual string PasswordKey
        {
            set { _authentication.PasswordKey = value; }
        }

        public virtual bool StoreUsername(string username)
        {
            return _authentication.StoreUsername(username);
        }

        public virtual bool StorePassword(string password)
        {
            return _authentication.StorePassword(password);
        }

        #endregion
    }
}