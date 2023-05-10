 // Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement
// under which you licensed this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.ExtensionMethods;
using Crestron.Panopto.Common.Interfaces;
using Crestron.Panopto.Common.StandardCommands;
using Crestron.Panopto.Common.Transports;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Crestron.Panopto.Common.Events;
using Crestron.Panopto.Common.Logging;

namespace Crestron.Panopto.Common.BasicDriver
{
    /// <summary>
    /// The abstract class that represent all drivers.
    /// It provides a default implementation of the following component interfaces:
    /// <para />IBasicInformation, IConnection2, IBasicLogger, IBasicInformation2, IDisposable, ISupportedCommandsHelper, and IModelFileSupport.
    /// <para />Drivers should not override anything here and should instead override the protocol class unless specified otherwise.
    /// </summary>
    public abstract class ABasicDriver :
        IBasicInformation, IConnection2, IBasicLogger, IBasicInformation2, IDisposable,
        ISupportedCommandsHelper, ICustomCommandCollection2,
        ILocalizedDevice, IFeedbackInformation
        //, ICloudReporting
    {
        /// <summary>
        /// A logger object that should not be referenced within drivers.
        /// This was introduced for more logging levels in the framework.
        /// </summary>
        protected Logger Logger;

        /*
        /// <summary>
        /// Used by this class to report changes to the Crestron Cloud
        /// and to allow the cloud to control the driver
        /// </summary>
        private CloudReporter _cloudDeviceReporter;
        */

        /// <summary>
        /// The device type of this driver from the embedded JSON file
        /// </summary>
        private Crestron.Panopto.Common.Enums.DeviceTypes _deviceType;

        /// <summary>
        /// Expected parameters when a user uses the console command CCDLoggingLevel
        /// </summary>
        private static string _debugLoggingLevelParamater = "debug";
        private static string _warningLoggingLevelParamater = "warning";
        private static string _errorLoggingLevelParamater = "error";

        private bool _disposed = false;
        private IAuthentication _authentication;
        private IAuthentication2 _authentication2;

        private Dictionary<string, string> _storedStringUserAttributes;
        private Dictionary<string, ushort> _storedUshortUserAttributes;
        private Dictionary<string, bool> _storedBooleanUserAttributes;

        private ABaseDriverProtocol _deviceProtocol;

        /// <summary>
        /// Used internally for mapping to the driver's protocol class.
        /// <para />The driver's protocol class which is responsible for handling all transmitted and received messages.
        /// </summary>
        protected ABaseDriverProtocol DeviceProtocol
        {
            get { return _deviceProtocol; }
            set
            {
                _deviceProtocol = value;
                if (_deviceProtocol != null)
                {
                    _deviceProtocol.DriverID = DriverID;
                    _deviceProtocol.BaseDeviceProtocolInitialized -= ProcessStoredUserAttributes;
                    _deviceProtocol.BaseDeviceProtocolInitialized += ProcessStoredUserAttributes;

                    // Provide the protocol the logger created in this class
                    _deviceProtocol.Logger = Logger;
                }
            }
        }
        
        private IEnumerable<PropertyInfo> _supportBooleans;

        internal IEnumerable<PropertyInfo> SupportBooleans
        {
            get
            {
                if (_supportBooleans.DoesNotExist())
                {
                    _supportBooleans = AbstractClassType.GetProperties().Where(x => x.PropertyType == typeof(bool) && x.Name.Contains("Supports"));
                }
                return _supportBooleans;
            }
        }

        /// <summary>
        /// The transport used for communication with the device which is responsible for transmitting and receiving messages.
        /// <para />This should be set within the driver's constructor to a class that inmplements ATransportDriver.
        ///
        /// [[[example]]]
        /// </summary>
        protected ISerialTransport ConnectionTransport;

        /// <summary>
        /// <para />The common driver data between all device types.
        /// This will be set automatically by ConvertJsonFileToDriverData in each device-type library.
        /// <para />This should not be used or referenced - use the following instead:
        /// <para />Display - DisplayData
        /// <para />Cable Box - CableBoxData
        /// <para />Video Server - VideoServerData
        /// <para />Bluray Player - BlurayPlayerData
        /// <para />AV Receiver - AvrData
        /// </summary>
        protected internal BaseRootObject DriverData;

        [System.Obsolete("This has been deprecated and is not used", false)]
        protected static string DeviceTypeName;

        [System.Obsolete("This has been deprecated and is not used", false)]
        protected string JsonString;

        /// <summary>
        /// Used internally for logging.
        /// Value is auto-assigned at run-time and should not be changed.
        /// </summary>
        protected int DriverID;

        private static int _driversLoaded = 0;
        private static bool _consoleCommandsLoaded;

        private delegate void DriverConsoleCommand(ConsoleCommandType command, string args);
        private DriverConsoleCommand InternalDriverConsoleCommandHandler;

        private static event DriverConsoleCommand DriverConsoleCommandEvent;

        private enum ConsoleCommandType
        {
            RADInfo = 1,
            TxDebug = 2,
            RxDebug = 3,
            StackTrace = 4,
            General = 5,
            LoggingLevel = 6,
            DriverStates = 7
        }

        private List<SupportedCommand> _supportedCommands;

        /// <summary>
        /// Used internally by each device-type library.
        /// <para />This is the CType of the device-type class that implements this class
        /// </summary>
        public abstract CType AbstractClassType { get; }

        /// <summary>
        /// Used internally by each device-type library to parse the JSON driver data and to set the BaseRootObject.
        /// <para />Drivers should not override this and applications should not invoke it.
        /// </summary>
        /// <param name="jsonString">The serialized JSON object</param>
        public abstract void ConvertJsonFileToDriverData(string jsonString);

        /// <summary>
        /// The ID that a driver will use when forming messages to the device.
        /// <para />This has to be set before the driver is initialized, otherwise the user attribute must be used to set it after initialization.
        /// <para />All drivers that support this field must also support the user attribute using the type "DeviceId"
        /// </summary>
        public byte Id { get; set; }

        /// <summary>
        /// The default TCP/UDP port that will be used by the driver.
        /// <para />This should be used when initalizing an Ethernet driver unless a different value is needed.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.Api.Communication.Port
        /// </summary>
        public int Port { get { return DriverData.CrestronSerialDeviceApi.Api.Communication.Port; } }

        /// <summary>
        /// Specifies if the driver supports auto-reconnect if communication with the device is lost.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.Api.Communication.EnableAutoReconnect
        /// </summary>
        protected bool EnableAutoReconnect { get { return DriverData.CrestronSerialDeviceApi.Api.Communication.EnableAutoReconnect; } }

        /// <summary>
        /// Constructor
        /// <para />This will do the following:
        /// <para />Load the driver JSON file (the first file found that ends with .JSON)
        /// <para />(S# Pro Only)Add console commands if they have not already been loaded
        /// <para />Set the driver ID used for logging
        /// </summary>
        public ABasicDriver()
        {
            Logger = new Logger(Log);
            Logger.CurrentLevel = LoggingLevel.Error;

            _supportedCommands = new List<SupportedCommand>();

            LoadDriverData();
        //    SetupCloudReporting();
            SetSupportedCommands();

            if (!_consoleCommandsLoaded && CrestronEnvironment.RuntimeEnvironment == eRuntimeEnvironment.SimplSharpPro)
            {
                CrestronConsole.AddNewConsoleCommand(ConsoleLoggingToggle, "CCDLogging", "Toggles general logging for the driver", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleTxDebugToggle, "CCDTxDebug", "Toggles connection transport (TX) logging", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleRxDebugToggle, "CCDRxDebug", "Toggles connection transport (RX) logging", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleStackTraceToggle, "CCDStackTrace", "Toggles stack trace printing when exceptions occur", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleRadInfo, "CCDInfo", "Prints out information regarding all the loaded drivers", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleLoggingLevel, "CCDLoggingLevel", "Sets the logging level for the driver", ConsoleAccessLevelEnum.AccessProgrammer);
                CrestronConsole.AddNewConsoleCommand(ConsoleDriverStates, "CCDStates", "Prints out driver states", ConsoleAccessLevelEnum.AccessProgrammer);
                _consoleCommandsLoaded = true;
            }

            DriverID = ++_driversLoaded;
            InternalDriverConsoleCommandHandler = new DriverConsoleCommand(DriverConsoleCommandCallback);
            DriverConsoleCommandEvent += InternalDriverConsoleCommandHandler;
            Connected = false;
            IsAuthenticated = false;

            _storedStringUserAttributes = new Dictionary<string, string>();
            _storedUshortUserAttributes = new Dictionary<string, ushort>();
            _storedBooleanUserAttributes = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Used internally by all device-type libraries when they are initialized.
        /// </summary>
        /// <param name="driverData">The BaseRootObject</param>
        public void Initialize(BaseRootObject driverData)
        {
            DriverData = driverData;
            CrestronDataStoreWrapper dataStoreWrapper = new CrestronDataStoreWrapper();
            dataStoreWrapper.Initialize();
            _authentication = new Authentication(DriverData.CrestronSerialDeviceApi.Api.Communication.Authentication, dataStoreWrapper);
            _authentication2 = new Authentication2(DriverData.CrestronSerialDeviceApi.Api.Communication.Authentication);
        }

        private void PrintSupportsBooleans()
        {
            var supportStatements = SupportBooleans;
            foreach (var info in supportStatements)
            {
                try
                {
                    bool value = (bool)info.GetValue(this, null);
                    var name = info.Name;
                    Log(string.Format("{0} = {1}", name, value));
                }
                catch (Exception e)
                {
                    Log(string.Format("Error Printing SupportsBooleans : {0}", e.Message));
                }
            }
        }

        private void SetSupportedCommands()
        {
            _supportedCommands = new List<SupportedCommand>();

            SetMainSupportedCommmands();
            SetSupportedCustomCommands();
            SetSupportedInputSources();
            SetSupportedArrowKeys();
            SetSupportedColorButtons();
            SetSupportsLetterButtons();
        }

        private void SetMainSupportedCommmands()
        {
            var deviceSupportNode = DriverData.CrestronSerialDeviceApi.DeviceSupport;

            // Use of foreach is because this may be less expensive that using Linq to create a list of keys
            // Since we still need to check if that value for the key is (bool) True
            foreach (var keyValuePair in deviceSupportNode)
            {
                if (keyValuePair.Value)
                {
                    object standardCommands = null;
                    SupportedCommandsMapping.SupportedFeatureToStandardCommandMapping.TryGetValue((int)keyValuePair.Key, out standardCommands);
                    if (standardCommands.Exists())
                    {
                        if (standardCommands is StandardCommandsEnum)
                        {
                            var standardCommand = (StandardCommandsEnum)standardCommands;
                            _supportedCommands.Add(new SupportedCommand(standardCommand.ToString(), standardCommand));
                        }
                        else if (standardCommands is List<StandardCommandsEnum>)
                        {
                            var standardCommandList = (List<StandardCommandsEnum>)standardCommands;
                            for (int i = 0; i < standardCommandList.Count; i++)
                            {
                                _supportedCommands.Add(new SupportedCommand(standardCommandList[i].ToString(), standardCommandList[i]));
                            }
                        }
                    }
                }
            }
        }

        private void SetSupportedInputSources()
        {
            var videoInputs = GetControllableVideoInputs();
            if (videoInputs.Exists())
            {
                for (int i = 0; i < videoInputs.Count; i++)
                {
                    _supportedCommands.Add(new SupportedCommand(videoInputs[i].ToString(), videoInputs[i]));
                }
            }
        }

        private void SetSupportedArrowKeys()
        {
            var arrowKeysSupported = GetControllableArrowKeys();
            if (arrowKeysSupported.Exists())
            {
                for (int i = 0; i < arrowKeysSupported.Count; i++)
                {
                    _supportedCommands.Add(new SupportedCommand(arrowKeysSupported[i].ToString(), arrowKeysSupported[i]));
                }
            }
        }

        private void SetSupportedColorButtons()
        {
            var colorButtonsSupported = GetControllableColorButtons();
            if (colorButtonsSupported.Exists())
            {
                for (int i = 0; i < colorButtonsSupported.Count; i++)
                {
                    _supportedCommands.Add(new SupportedCommand(colorButtonsSupported[i].ToString(), colorButtonsSupported[i]));
                }
            }
        }

        private void SetSupportsLetterButtons()
        {
            var letterButtonsSupported = GetControllableLetterButtons();
            if (letterButtonsSupported.Exists())
            {
                for (int i = 0; i < letterButtonsSupported.Count; i++)
                {
                    _supportedCommands.Add(new SupportedCommand(letterButtonsSupported[i].ToString(), letterButtonsSupported[i]));
                }
            }
        }

        private void SetSupportedCustomCommands()
        {
            var customCommands = CustomCommandNames;
            for (int i = 0; i < customCommands.Count; i++)
            {
                _supportedCommands.Add(new SupportedCommand(customCommands[i], StandardCommandsEnum.Nop));
            }
        }

        /// <summary>
        /// Used internally by all device-type libraries.
        /// <para />Drivers should not override this.
        /// </summary>
        /// <returns>A list of controllable video inputs supported by the driver</returns>
        protected virtual List<StandardCommandsEnum> GetControllableVideoInputs()
        {
            return null;
        }

        /// <summary>
        /// Used internally by all device-type libraries.
        /// <para />Drivers should not override this.
        /// </summary>
        /// <returns>A list of controllable video inputs supported by the driver</returns>
        protected virtual List<StandardCommandsEnum> GetControllableArrowKeys()
        {
            return null;
        }

        /// <summary>
        /// Used internally by all device-type libraries.
        /// <para />Drivers should not override this.
        /// </summary>
        /// <returns>A list of controllable video inputs supported by the driver</returns>
        protected virtual List<StandardCommandsEnum> GetControllableLetterButtons()
        {
            return null;
        }

        /// <summary>
        /// Used internally by all device-type libraries.
        /// <para />Drivers should not override this.
        /// </summary>
        /// <returns>A list of controllable video inputs supported by the driver</returns>
        protected virtual List<StandardCommandsEnum> GetControllableColorButtons()
        {
            return null;
        }

        private void LoadDriverData()
        {
            ConvertJsonFileToDriverData(ReadJsonFile());
        }

        /// <summary>
        /// Used internally by all device-type libraries.
        /// <para />Drivers should not override this.
        /// </summary>
        /// <returns>JsonSerializerSettings used for deserializing the JSON driver data.</returns>
        protected JsonSerializerSettings CreateSerializerSettings()
        {
            var deviceSupportNodeConverter = CreateDeviceSupportConverter();

            var serializerSettings = new JsonSerializerSettings();

            serializerSettings.Converters.Add(new AuthenticationJsonConverter());
            serializerSettings.Converters.Add(new StandardCommandConverter());

            if (deviceSupportNodeConverter.Exists())
            {
                serializerSettings.Converters.Add(deviceSupportNodeConverter);
            }

            return serializerSettings;
        }

        /// <summary>
        /// Used internally by all device-type libraries.
        /// <para />Drivers should not override this.
        /// </summary>
        /// <returns>JsonConverter used by CreateSerializerSettings() to handle the DeviceSupport data in the JSON driver data.</returns>
        protected virtual JsonConverter CreateDeviceSupportConverter()
        {
            // No base implementation
            return null;
        }

        private string ReadJsonFile()
        {
            string dataFile = string.Empty;
            string resourceName = string.Empty;

            Assembly assembly = AbstractClassType.Assembly;
            resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

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
                if (EnableLogging)
                {
                    Log("Driver JSON file is missing");
                }
            }
            return dataFile;
        }

        private void ProcessStoredUserAttributes()
        {
            var userAttributeKeys = _storedBooleanUserAttributes.Keys.ToList();
            for (int i = 0; i < userAttributeKeys.Count; i++)
            {
                SetUserAttribute(userAttributeKeys[i], _storedBooleanUserAttributes[userAttributeKeys[i]]);
            }

            userAttributeKeys = _storedUshortUserAttributes.Keys.ToList();
            for (int i = 0; i < userAttributeKeys.Count; i++)
            {
                SetUserAttribute(userAttributeKeys[i], _storedUshortUserAttributes[userAttributeKeys[i]]);
            }

            userAttributeKeys = _storedStringUserAttributes.Keys.ToList();
            for (int i = 0; i < userAttributeKeys.Count; i++)
            {
                SetUserAttribute(userAttributeKeys[i], _storedStringUserAttributes[userAttributeKeys[i]]);
            }

            _storedBooleanUserAttributes.Clear();
            _storedStringUserAttributes.Clear();
            _storedUshortUserAttributes.Clear();
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
                case ConsoleCommandType.LoggingLevel:
                    SetLoggingLevel(args);
                    break;

                case ConsoleCommandType.DriverStates:
                    PrintDriverStates(args);
                    break;

            }
        }


        private void ConsoleLoggingLevel(string args)
        {
            if (DriverConsoleCommandEvent.Exists())
            {
                DriverConsoleCommandEvent(ConsoleCommandType.LoggingLevel, args);
            }
        }

        private void ConsoleRadInfo(string args)
        {
            if (DriverConsoleCommandEvent.Exists())
            {
                DriverConsoleCommandEvent(ConsoleCommandType.RADInfo, args);
            }
        }

        private void ConsoleLoggingToggle(string args)
        {
            if (DriverConsoleCommandEvent.Exists())
            {
                DriverConsoleCommandEvent(ConsoleCommandType.General, args);
            }
        }

        private void ConsoleTxDebugToggle(string args)
        {
            if (DriverConsoleCommandEvent.Exists())
            {
                DriverConsoleCommandEvent(ConsoleCommandType.TxDebug, args);
            }
        }

        private void ConsoleRxDebugToggle(string args)
        {
            if (DriverConsoleCommandEvent.Exists())
            {
                DriverConsoleCommandEvent(ConsoleCommandType.RxDebug, args);
            }
        }

        private void ConsoleStackTraceToggle(string args)
        {
            if (DriverConsoleCommandEvent.Exists())
            {
                DriverConsoleCommandEvent(ConsoleCommandType.StackTrace, args);
            }
        }

        private void ConsoleDriverStates(string args)
        {
            if (DriverConsoleCommandEvent.Exists())
            {
                DriverConsoleCommandEvent(ConsoleCommandType.DriverStates, args);
            }
        }

        private void PrintDriverStates(string args)
        {
            try
            {
                if (IsValidDriverID(args))  //print single driver states
                {
                    CrestronConsole.PrintLine("\u000D---------- Driver States for DriverID {0}----------", DriverID);
                    CrestronConsole.PrintLine("Driver BaseModel: {0} - {1}", Manufacturer, BaseModel);

                    //printing all values including Supported Features and supported models
                    //ex: ccdstates 1 all
                    if (args.Contains(' ') && args.Split(' ')[1].Equals("all"))
                    {
                        PrintProperties(this, 1, true);
                        if (this.DeviceProtocol != null)
                        {
                            PrintProperties(this.DeviceProtocol, 1, true);
                        }
                    }
                    else
                    {
                        //not printing Supported Features
                        //ex: ccdstates 1
                        PrintProperties(this, 1, false);
                        if (this.DeviceProtocol != null)
                        {
                            PrintProperties(this.DeviceProtocol, 1, false);
                        }
                    }
                    CrestronConsole.PrintLine("---------- Driver States for DriverID {0} End ----------\u000D", DriverID);
                }
                //print states for all drivers loaded
                //ex: ccdstates
                else if (string.IsNullOrEmpty(args))
                {
                    CrestronConsole.PrintLine("\u000D---------- Driver States for DriverID {0}----------", DriverID);
                    CrestronConsole.PrintLine("Driver BaseModel: {0} - {1}", Manufacturer, BaseModel);
                    PrintProperties(this, 1, false);
                    if (this.DeviceProtocol != null)
                    {
                        PrintProperties(this.DeviceProtocol, 1, false);
                    }
                    CrestronConsole.PrintLine("---------- Driver States for DriverID {0} End ----------\u000D", DriverID);
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error occured printing driver states " + e.Message);
            }
        }

        private string _previousDeclaringType = string.Empty;
        private void PrintProperties(object obj, int indent, bool printSupportsStatements)
        {
            try
            {
                CType objType = obj.GetType();
                var properties = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                string indentString = new string('\t', indent);
                foreach (var property in properties)
                {
                    string currentDeclaringType = property.DeclaringType.FullName.ToString();

                    if (!currentDeclaringType.Equals(_previousDeclaringType))
                    {
                        CrestronConsole.PrintLine("\nDeclaringType - {0}:", currentDeclaringType);
                        _previousDeclaringType = currentDeclaringType;
                    }
                    //ignoring supports statements as PrintSupportsBooleans() already prints them upon loading driver
                    if ((property.Name.Contains("Support") && !printSupportsStatements)
                        || !property.CanRead)
                    {
                        continue;
                    }
                    var propValue = property.GetValue(obj, null);
                    var propList = propValue as System.Collections.IList;
                    if (propList != null)
                    {
                        CrestronConsole.PrintLine("{0}{1}:", indentString, property.Name);
                        if (propList.Count == 0)
                        {
                            CrestronConsole.PrintLine("\t\t\t{0}Empty List", indentString);
                        }
                        else foreach (var val in propList)
                            {
                                if (val is string || val is Enum)
                                {
                                    CrestronConsole.PrintLine("\t\t\t{0}{1}", indentString, val);
                                }
                                else
                                {
                                    //recursively print lists
                                    PrintProperties(val, indent + 3, printSupportsStatements);
                                    CrestronConsole.Print("\n");
                                }
                            }
                    }
                    //Getting Child Properties
                    else if (property.PropertyType.IsAnsiClass
                               && !property.PropertyType.IsEnum
                               && !property.PropertyType.FullName.StartsWith("System.")
                               && !property.Name.Contains("AbstractClassType")
                               && propValue != null)
                    {
                        CrestronConsole.PrintLine("{0}{1}:", indentString, property.Name);
                        CType childType = propValue.GetType();
                        var childValue = childType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        foreach (var value in childValue)
                        {
                            var name = value.Name;
                            var actualVal = value.GetValue(propValue, null);
                            actualVal = actualVal == null ? "null" : actualVal;
                            actualVal = actualVal.Equals(String.Empty) ? "String.Empty" : actualVal;
                            CrestronConsole.PrintLine("\t\t\t{0}{1}: {2}", indentString, name, actualVal);
                        }
                    }
                    //printing string values, bools, int, etc
                    else
                    {
                        propValue = propValue == null ? "null" : propValue;
                        propValue = propValue.Equals(String.Empty) ? "String.Empty" : propValue;
                        CrestronConsole.PrintLine("{0}{1}: {2}", indentString, property.Name, propValue.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Issue occured printing driver properties: {0}", e.Message);
            }
        }

        private void SetLoggingLevel(string args)
        {
            if (Logger != null &&
                IsValidDriverID(args))
            {
                var argsSplit = args.Split(' ');

                if (argsSplit.Length >= 2)
                {
                    if (argsSplit[1].Equals(_debugLoggingLevelParamater, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Logger.CurrentLevel = LoggingLevel.Debug;
                        CrestronConsole.PrintLine("Logging level set to Debug on driver ID {0}", DriverID);
                    }
                    else if (argsSplit[1].Equals(_warningLoggingLevelParamater, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Logger.CurrentLevel = LoggingLevel.Warning;
                        CrestronConsole.PrintLine("Logging level set to Warning on driver ID {0}", DriverID);
                    }
                    else if (argsSplit[1].Equals(_errorLoggingLevelParamater, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Logger.CurrentLevel = LoggingLevel.Error;
                        CrestronConsole.PrintLine("Logging level set to Error on driver ID {0}", DriverID);
                    }
                }
            }
        }

        private void ShowRadInfo(string args)
        {
            var assembly = Assembly.GetExecutingAssembly();

            CrestronConsole.PrintLine("\u000D--- Driver Info for Driver ID {0}---", DriverID);
            try
            {
                CrestronConsole.PrintLine("Driver: {0} - {1}", Manufacturer, BaseModel);
                CrestronConsole.PrintLine("Type: {0}", DriverData.CrestronSerialDeviceApi.GeneralInformation.DeviceType);
                
                CrestronConsole.PrintLine("GUID: {0}", Guid);
                CrestronConsole.PrintLine("SDK Version: {0}", DriverData.CrestronSerialDeviceApi.GeneralInformation.SdkVersion);
                CrestronConsole.PrintLine("Version: {0} ({1})", DriverVersion, VersionDate.Date);
                CrestronConsole.PrintLine("Using {0}", assembly.FullName);
                CrestronConsole.PrintLine("Using {0}", AbstractClassType.BaseType.Assembly.FullName);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Issue occured getting driver information: {0}", e);
            }
            CrestronConsole.PrintLine("--- Driver Info End ---\u000D");
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
            var state = args.Split(' ')[1];

            if (state.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private bool IsValidCommandParameter(string args)
        {
            if (args.Contains(' '))
            {
                var arguments = args.Split(' ');

                if (arguments[1].Equals("on", StringComparison.OrdinalIgnoreCase) ||
                    arguments[1].Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion Console Commands

        #region IBasicInformation Members

        /// <summary>
        /// The GUID of the driver.
        /// <para />This is no longer used.
        /// </summary>
        public Guid Guid
        {
            get
            { return DriverData.CrestronSerialDeviceApi.GeneralInformation.Guid; }
        }

        /// <summary>
        /// A description of the device the driver controls.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.GeneralInformation.Description
        /// </summary>
        public string Description
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.Description; }
        }

        /// <summary>
        /// The manufacturer of the device.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.GeneralInformation.Manufacturer
        /// </summary>
        public string Manufacturer
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.Manufacturer; }
        }

        /// <summary>
        /// The base model of the device this driver was created for.
        /// <para />The driver can work with other models specified by SupportModels.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.GeneralInformation.BaseModel
        /// </summary>
        public string BaseModel
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.BaseModel; }
        }

        /// <summary>
        /// The version of the driver.
        /// <para />The first octet specifies breaking changes by the driver.
        /// If the driver starts with 2.x.x.x and the framework libraries are 3.x.x.x
        /// then this driver may not work with those libraries.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.GeneralInformation.DriverVersion
        /// </summary>
        public string DriverVersion
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.DriverVersion; }
        }

        /// <summary>
        /// A list of supported Series of devices that is supported by this driver.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.GeneralInformation.SupportedSeries
        /// </summary>
        public List<string> SupportedSeries
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.SupportedSeries; }
        }

        /// <summary>
        /// A list of all models that the driver supports.
        /// <para />Although functionality will remain, certain models may have a different set of IO connectors than specified by the base model.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.GeneralInformation.SupportedModels
        /// </summary>
        public List<string> SupportedModels
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.SupportedModels; }
        }

        /// <summary>
        /// A date and time of when the driver was last modified.
        /// <para />This value is specified by the JSON driver data from the following node:
        /// <para />CrestronSerialDeviceApi.GeneralInformation.VersionDate
        /// </summary>
        public DateTime VersionDate
        {
            get { return DriverData.CrestronSerialDeviceApi.GeneralInformation.VersionDate; }
        }

        #endregion IBasicInformation Members

        #region Custom Command

        /// <summary>
        /// Method to send a custom command to the device.
        /// <para />This will call ConectionTransport.Send(command, null)
        /// </summary>
        /// <param name="command">The string that should be sent immediatly to the device.</param>
        public void SendCustomCommand(string command)
        {
            if (ConnectionTransport != null)
            {
                ConnectionTransport.Send(command, null);
            }
        }

        /// <summary>
        /// Enables the RxOut event.
        /// <para />If set to true then all data receieved will be provided by the RxOut event.
        /// <para />This is disabled by default.
        /// </summary>
        public bool EnableRxOut { get; set; }

        /// <summary>
        /// Sends strings sent from the device.
        /// </summary>
        CCriticalSection _rxLock = new CCriticalSection();

        private event Action<string> _rxOut;

        /// <summary>
        /// An event that provides data received by the driver.
        /// <para />This event is only active while EnableRxOut is true.
        /// </summary>
        public event Action<string> RxOut
        {
            add
            {
                try
                {
                    _rxLock.Enter();
                    _rxOut += value;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Crestron.Panopto.Common.BasicDriver.BasicDriver add _rxOut - {0}", e.ToString());
                }
                finally
                {
                    _rxLock.Leave();
                }
            }

            remove
            {
                try
                {
                    _rxLock.Enter();
                    _rxOut -= value;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Crestron.Panopto.Common.BasicDriver.BasicDriver remove _rxOut {0}", e.ToString());
                }
                finally
                {
                    _rxLock.Leave();
                }
            }
        }

        /// <summary>
        /// Used internally to call the RxOut event with the specified message.
        /// </summary>
        /// <param name="message">Data received from the device</param>
        protected virtual void SendRxOut(string message)
        {
            if (_rxOut != null && EnableRxOut)
            {
                _rxOut(message);
            }
        }

        #endregion Custom Command

        #region ICustomCommandCollection Members

        /// <summary>
        /// Adds a custom command to the driver that can be used by calling SendCustomCommandByName(string commandName).
        /// <para />Commands added do not persist after reboots or program resets.
        /// <para />Commands must be added after initialization.
        /// </summary>
        /// <param name="commandName">The friendly name of the command</param>
        /// <param name="commandValue">The value that should be sent to the device</param>
        /// <param name="parameters">Any parameters specified by the commandValue</param>
        public virtual void AddCustomCommand(string commandName, string commandValue, List<Parameters> parameters)
        {
            if (DeviceProtocol != null)
            {
                DeviceProtocol.AddCustomCommand(commandName, commandValue, parameters);
            }
            else
            {
                if (EnableLogging)
                {
                    Log("Unable to add custom commands before initialization");
                }
            }
        }

        /// <summary>
        /// Checks if a custom command exists in the internal collection.
        /// <para />This will search all added custom commands and custom commands defined in the JSON driver data in the node:
        /// <para />DriverData.CrestronSerialDeviceApi.Api.CustomCommands
        /// </summary>
        /// <param name="commandName">The friendly name of the command</param>
        /// <returns>True if it exists</returns>
        public virtual bool CheckIfCustomCommandExists(string commandName)
        {
            if (DeviceProtocol == null)
            {
                if (string.IsNullOrEmpty(commandName))
                    return false;

                return DriverData.CrestronSerialDeviceApi.Api.CustomCommands != null
                    && DriverData.CrestronSerialDeviceApi.Api.CustomCommands.Count > 0
                    && DriverData.CrestronSerialDeviceApi.Api.CustomCommands
                         .Any(cmd => (cmd.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                return DeviceProtocol.CheckIfCustomCommandExists(commandName);
            }
        }

        /// <summary>
        /// Returns a list of all added custom command names and custom commands defined in the JSON driver data in the node:
        /// <para />DriverData.CrestronSerialDeviceApi.Api.CustomCommands
        /// </summary>
        public virtual List<string> CustomCommandNames
        {
            get
            {
                if (DeviceProtocol == null)
                {
                    if ((DriverData.CrestronSerialDeviceApi.Api
                        .CustomCommands == null) ||
                        (DriverData.CrestronSerialDeviceApi.Api
                        .CustomCommands.Count == 0))
                        return new List<string>();

                    return (DriverData.CrestronSerialDeviceApi.Api
                        .CustomCommands.Select(cmd => cmd.Name).ToList());
                }
                else
                {
                    return DeviceProtocol.CustomCommandNames;
                }
            }
        }

        /// <summary>
        /// Removes a custom command from the driver.
        /// <para />Commands must be removed after initialization.
        /// </summary>
        /// <param name="commandName">The friendly name of the command to remove</param>
        /// <returns>True if the command was removed</returns>
        public virtual bool RemoveCustomCommandByName(string commandName)
        {
            if (DeviceProtocol != null)
            {
                return DeviceProtocol.RemoveCustomCommandByName(commandName);
            }
            else
            {
                if (EnableLogging)
                {
                    Log("Unable to remove custom commands before initialization");
                }
                return false;
            }
        }

        /// <summary>
        /// Sends the specified custom command.
        /// </summary>
        /// <param name="commandName">The friendly name of the command to remove</param>
        public virtual void SendCustomCommandByName(string commandName)
        {
            if (DeviceProtocol != null)
            {
                DeviceProtocol.SendCustomCommandByName(commandName);
            }
            else
            {
                if (EnableLogging)
                {
                    Log("Unable to send custom commands before initialization");
                }
            }
        }

        /// <summary>
        /// Sends the value specified immediatly to the transport.
        /// <para />This will call ConectionTransport.Send(command, null)
        /// </summary>
        /// <param name="commandValue"></param>
        public virtual void SendCustomCommandValue(string commandValue)
        {
            if (ConnectionTransport != null)
            {
                ConnectionTransport.Send(commandValue, null);
            }
            else
            {
                if (EnableLogging)
                {
                    Log("Unable to send custom commands before initialization");
                }
            }
        }

        #endregion ICustomCommandCollection Members

        #region Logging

        protected bool InternalEnableStackTrace;

        /// <summary>
        /// Enables stack traces (where applicable) in logged exceptions.
        /// <para />This will not print if EnableLogging is not true.
        /// </summary>
        public bool EnableStackTrace
        {
            set
            {
                InternalEnableStackTrace = value;
                if (DeviceProtocol != null)
                {
                    DeviceProtocol.EnableStackTrace = value;
                }
            }
            get { return InternalEnableStackTrace; }
        }

        /// <summary>
        /// Used internally to for enabling/disabling stack trace logging.
        /// </summary>
        protected bool InternalEnableTxDebug;

        /// <summary>
        /// Enables logging of all transmitted data to the device.
        /// <para />This will not print if EnableLogging is not true.
        /// </summary>
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

        /// <summary>
        /// Used internally to for enabling/disabling TX logging.
        /// </summary>
        protected bool InternalEnableRxDebug;

        /// <summary>
        /// Enables logging of all received data from the device.
        /// <para />This will not print if EnableLogging is not true.
        /// </summary>
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

        /// <summary>
        /// Used internally to for enabling/disabling RX logging.
        /// </summary>
        protected bool InternalEnableLogging;

        /// <summary>
        /// Enables logging on the driver.
        /// </summary>
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
                if (Logger != null)
                {
                    Logger.LoggingEnabled = value;
                }
                if (value)
                {
                    PrintSupportsBooleans();
                }
            }
            get { return InternalEnableLogging; }
        }

        /// <summary>
        /// Used internally to for determining how messages are logged.
        /// </summary>
        protected Action<string> InternalCustomLogger;

        /// <summary>
        /// Determines how messages are logged.
        /// <para />CrestronConsole.PrintLine is the default logger.
        /// </summary>
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

        /// <summary>
        /// Called by drivers to log a message.
        /// <para />Logged messages will contain the current tick count, driver ID, abstract class type, and the specified message.
        /// </summary>
        /// <param name="message">The message that should be logged</param>
        protected void Log(string message)
        {
            if (!InternalEnableLogging) return;
            message = string.Format("{0}::{1}::({2}) {3} : {4}",
                        CrestronEnvironment.TickCount,
                        DateTime.Now,
                        DriverID,
                        AbstractClassType.Name,
                        message);

            if (InternalCustomLogger == null)
            {
                CrestronConsole.PrintLine(message);
            }
            else
            {
                InternalCustomLogger(message + "\n");
            }
        }

        /// <summary>
        /// Helper method to log a command not being supported by the driver.
        /// </summary>
        /// <param name="commandName">The command that is not supported</param>
        protected void LogCommandNotSupported(string commandName)
        {
            if (EnableLogging)
            {
                Log(string.Format("The command {0} is not supported.", commandName));
            }
        }

        /// <summary>
        /// Helper method to log the protocol not being initialized.
        /// </summary>
        /// <param name="operation"></param>
        protected void LogProtocolNotInitialized(string operation)
        {
            if (EnableLogging)
            {
                Log(string.Format("The operation {0} failed due to the driver not being initialized", operation));
            }
        }

        private void SetLoggingLevel(LoggingLevel level)
        {
            if (Logger != null)
            {
                Logger.CurrentLevel = level;
            }
        }

        #endregion Logging

        #region IDisposable Members

        /// <summary>
        /// Disposes the driver.
        /// <para />This will also stop the transport connection and dispose DeviceProtocol.
        /// <para />This should be called last when destroying a driver.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                DriverConsoleCommandEvent -= InternalDriverConsoleCommandHandler;
                if (ConnectionTransport.Exists())
                {
                    ConnectionTransport.Stop();
                }
                if (DeviceProtocol.Exists())
                {
                    DeviceProtocol.BaseDeviceProtocolInitialized -= ProcessStoredUserAttributes;
                    DeviceProtocol.FakeFeedbackForCommand -= OnFakeFeedbackChanged;
                    DeviceProtocol.RxOut -= SendRxOut;

                    DeviceProtocol.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion IDisposable Members

        #region IConnection2 Members

        /// <summary>
        /// Specifies if the driver supportes secure communications with the device.
        /// <para />Refer to IAuthentication2.Required to see if authentication is required to communicate with the device.
        /// <para />This value is specified by the JSON driver data - the following type must not be AuthenticationTypes.NONE
        /// <para />CrestronSerialDeviceApi.Api.Communication.Authentication.Type
        /// </summary>
        public bool IsSecure
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.Api.Communication.Authentication.Exists()
                       && DriverData.CrestronSerialDeviceApi.Api.Communication.Authentication.Type != AuthenticationTypes.NONE;
            }
        }

        /// <summary>
        /// Specifies the driver is currently connected to the device.
        /// <para /> Ethernet driver will set this to true when the socket state is OK.
        /// <para /> HTTP drivers will set this to true when communication is possible with the device.
        /// <para /> COM and CEC drivers will set this value if they receive any data from the device.
        /// <para /> IR drivers will not set this to true.
        /// </summary>
        public bool Connected { get; protected set; }

        /// <summary>
        /// Specifies if the driver supports disconnecting from the device.
        /// <para />This value is specified by the JSON driver data -
        /// the DeviceSupport dictionary must contain CommonFeatureSupport.SupportsDisconnect as a key with a value of true.
        /// <para />CrestronSerialDeviceApi.DeviceSupport
        /// </summary>
        public virtual bool SupportsDisconnect
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(CommonFeatureSupport.SupportsDisconnect)
                    && DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsDisconnect];
            }
        }

        /// <summary>
        /// This will disconnect the driver from the device, if supported.
        /// <para />Driver developers - this will call ConnectionTransport.Stop()
        /// </summary>
        public virtual void Disconnect()
        {
            if (ConnectionTransport.Exists())
            {
                ConnectionTransport.Stop();
            }
        }

        /// <summary>
        /// Specifies if the driver supports reconnecting to the device.
        /// <para />This value is specified by the JSON driver data -
        /// the DeviceSupport dictionary must contain CommonFeatureSupport.SupportsReconnect as a key with a value of true.
        /// <para />CrestronSerialDeviceApi.DeviceSupport
        /// </summary>
        public virtual bool SupportsReconnect
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(CommonFeatureSupport.SupportsReconnect)
                    && DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsReconnect];
            }
        }

        /// <summary>
        /// This will disconnect the driver from the device and then connect to it, if supported.
        /// <para />Driver developers - this will call ConnectionTransport.Stop() then ConnectionTransport.Start()
        /// </summary>
        public virtual void Reconnect()
        {
            if (ConnectionTransport.Exists())
            {
                if (ConnectionTransport.IsConnected)
                {
                    ConnectionTransport.Stop();
                }
                ConnectionTransport.Start();
            }
        }

        /// <summary>
        /// This will connect the driver to the device, if supported.
        /// <para />Driver developers - this will call ConnectionTransport.Start()
        /// </summary>
        public virtual void Connect()
        {
            if (ConnectionTransport.Exists())
            {
                ConnectionTransport.Start();
            }
        }

        #endregion IConnection2 Members

        #region IAuthentication Members

        /// <summary>
        /// Specifies if the driver supports using a username for authentication with the device.
        /// <para />This value is specified by the JSON driver data -
        /// the DeviceSupport dictionary must contain CommonFeatureSupport.SupportsUsername as a key with a value of true.
        /// <para />CrestronSerialDeviceApi.DeviceSupport
        /// </summary>
        public bool SupportsUsername
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(
                    CommonFeatureSupport.SupportsUsername)
                       && DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsUsername];
            }
        }

        /// <summary>
        /// Deprecated.
        /// </summary>
        public string UsernameMask
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// When applications store a username value in CrestronDataStore with a specific key, this key can be supplied to the driver to use
        /// when authenticating with the device or when StoreUsername(string username) is called to store the username.
        /// <para />Driver developers - this must be implemented if the driver supports authentication with the device.
        /// base.UsernameKey must be called within the driver overridden method.
        /// </summary>
        public virtual string UsernameKey
        {
            set
            {
                if (_authentication != null)
                { _authentication.UsernameKey = value; }
            }
        }

        /// <summary>
        /// Specifies if the driver supports using a password for authentication with the device.
        /// <para />This value is specified by the JSON driver data -
        /// the DeviceSupport dictionary must contain CommonFeatureSupport.SupportsPassword as a key with a value of true.
        /// <para />CrestronSerialDeviceApi.DeviceSupport
        /// </summary>
        public bool SupportsPassword
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(
                    CommonFeatureSupport.SupportsPassword)
                       && DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsPassword];
            }
        }

        /// <summary>
        /// Deprecated.
        /// </summary>
        public string PasswordMask
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// When applications store a password value in CrestronDataStore with a specific key, this key can be supplied to the driver to use
        /// when authenticating with the device or when StorePassword(string password) is called to store the password.
        /// <para />Driver developers - this must be implemented in the driver if the driver supports authentication with the device.
        /// base.PasswordKey must be called within the driver overridden method.
        /// </summary>
        public virtual string PasswordKey
        {
            set { if (_authentication != null) { _authentication.PasswordKey = value; } }
        }

        /// <summary>
        /// Stores the specified username in CrestronDataStore using the key supplied by UsernameKey.
        /// </summary>
        /// <param name="username">The username value to be stored in CrestronDataStore.</param>
        /// <returns>True if the value was stored</returns>
        public virtual bool StoreUsername(string username)
        {
            return SupportsUsername && _authentication != null && _authentication.StoreUsername(username);
        }

        /// <summary>
        /// Stores the specified password in CrestronDataStore using the key supplied by PasswordKey.
        /// </summary>
        /// <param name="username">The username value to be stored in CrestronDataStore.</param>
        /// <returns>True if the value was stored</returns>
        public virtual bool StorePassword(string password)
        {
            return SupportsPassword && _authentication != null && _authentication.StorePassword(password);
        }

        #endregion IAuthentication Members

        #region IAuthentication2 Members

        /// <summary>
        /// Allows applications to set the username the driver should use when authenticating with the device.
        /// This can be set before or after initializing and connecting the driver.
        /// If a UsernameKey is supplied, then this value will not be used unless there is no username in CrestronDataStore.
        /// <para />Driver developers - this must be implemented in the driver if the driver supports authentication with the device.
        /// </summary>
        /// <param name="username">The username to use for authentication with the device.</param>
        public virtual void OverrideUsername(string username)
        {
            UsernameKey = username;
        }

        /// <summary>
        /// Allows applications to set the password the driver should use when authenticating with the device.
        /// This can be set before or after initializing and connecting the driver.
        /// If a PasswordKey is supplied, then this value will not be used unless there is no password in CrestronDataStore.
        /// <para />Driver developers - this must be implemented in the driver if the driver supports authentication with the device.
        /// </summary>
        /// <param name="username">The username to use for authentication with the device.</param>
        public virtual void OverridePassword(string password)
        {
            PasswordKey = password;
        }

        /// <summary>
        /// The default username the driver will use when authenticating with the device.
        /// </summary>
        public virtual string DefaultUsername
        {
            get { return _authentication2 == null ? string.Empty : _authentication2.DefaultUsername; }
        }

        /// <summary>
        /// The default password the driver will use when authenticating with the device.
        /// </summary>
        public virtual string DefaultPassword
        {
            get { return _authentication2 == null ? string.Empty : _authentication2.DefaultPassword; }
        }

        /// <summary>
        /// Specifies if authentication is required for communication with the device.
        /// </summary>
        public virtual bool Required
        {
            get { return _authentication2 != null && _authentication2.Required; }
        }

        /// <summary>
        /// Specifies if the driver is authenticated with the device.
        /// <para />Driver developers - this must be set within the driver and the Authentication event must be invoked.
        /// </summary>
        public bool IsAuthenticated { get; protected set; }

        #endregion IAuthentication2 Members

        #region IBasicInformation2 Members

        /// <summary>
        /// Sets a user attribute to the supplied value.
        /// This can be called before or after intialization.
        /// </summary>
        /// <param name="attributeId">The attribute ID</param>
        /// <param name="attributeValue">The attribute user-defined value</param>
        public void SetUserAttribute(string attributeId, string attributeValue)
        {
            if (DeviceProtocol.Exists())
            {
                DeviceProtocol.SetUserAttribute(attributeId, attributeValue);
            }
            else
            {
                _storedStringUserAttributes[attributeId] = attributeValue;
                if (EnableLogging)
                {
                    Log("Stored string user attribute internally - this will be set automatically once the driver is initialized");
                }
            }
        }

        /// <summary>
        /// Sets a user attribute to the supplied value.
        /// This can be called before or after intialization.
        /// </summary>
        /// <param name="attributeId">The attribute ID</param>
        /// <param name="attributeValue">The attribute user-defined value</param>
        public void SetUserAttribute(string attributeId, bool attributeValue)
        {
            if (DeviceProtocol.Exists())
            {
                DeviceProtocol.SetUserAttribute(attributeId, attributeValue);
            }
            else
            {
                _storedBooleanUserAttributes[attributeId] = attributeValue;

                if (EnableLogging)
                {
                    Log("Stored boolean user attribute internally - this will be set automatically once the driver is initialized");
                }
            }
        }

        /// <summary>
        /// Sets a user attribute to the supplied value.
        /// This can be called before or after intialization.
        /// </summary>
        /// <param name="attributeId">The attribute ID</param>
        /// <param name="attributeValue">The attribute user-defined value</param>
        public void SetUserAttribute(string attributeId, ushort attributeValue)
        {
            if (DeviceProtocol.Exists())
            {
                DeviceProtocol.SetUserAttribute(attributeId, attributeValue);
            }
            else
            {
                _storedUshortUserAttributes[attributeId] = attributeValue;
                if (EnableLogging)
                {
                    Log("Stored ushort user attribute internally - this will be set automatically once the driver is initialized");
                }
            }
        }

        /// <summary>
        /// Provides a list of avaialble user attributes.
        /// <para /> This collection is defined by the following JSON driver data class:
        /// <para /> DriverData.CrestronSerialDeviceApi.UserAttributes
        /// </summary>
        /// <returns>List of user attributes</returns>
        public List<UserAttribute> RetrieveUserAttributes()
        {
            List<UserAttribute> userAttributes = new List<UserAttribute>();

            if (DriverData.Exists() &&
                DriverData.CrestronSerialDeviceApi.Exists() &&
                DriverData.CrestronSerialDeviceApi.UserAttributes.Exists())
            {
                userAttributes = DriverData.CrestronSerialDeviceApi.UserAttributes;
            }
            else
            {
                if (EnableLogging)
                {
                    Log("No user attributes found in driver");
                }
            }

            return userAttributes;
        }

        #endregion IBasicInformation2 Members

        #region ISupportedCommandsHelper

        /// <summary>
        /// Provides a list of supported commands that the driver supports.
        /// </summary>
        public virtual List<SupportedCommand> SupportedCommands { get { return _supportedCommands == null ? new List<SupportedCommand>() : _supportedCommands; } }

        #endregion ISupportedCommandsHelper

        #region ICustomCommandCollection2 Members

        public void SendCustomCommandByName(string commandName, CommandAction action)
        {
            if (DeviceProtocol != null)
            {
                DeviceProtocol.SendCustomCommandByName(commandName, action);
            }
            else
            {
                if (EnableLogging)
                {
                    Log("Unable to send custom commands before initialization");
                }
            }
        }

        #endregion

        #region ILocalizedDevice Members

        /// <summary>
        /// Retrieves all the localized strings (and their IDs) used by this device.
        /// </summary>
        /// <param name="culture">
        /// Specifies the language code and country region code to be retrieved.
        /// The string must formatted as the "languagecode2" as defined by the ISO 639-1 standard,
        /// followed by a hyphen and then the "country/regioncode2" as defined by the ISO 3166.
        /// <para>
        /// For example, "en-US" for English in the United States.
        /// </para>
        /// <para>
        /// If the specified culture is not supported, this will return null.
        /// </para>
        /// </param>
        /// <para>
        /// Keys of 0 are not supported.
        /// Any property that returns 0 as a localized string ID indicates an empty string.
        /// </para>
        public virtual IEnumerable<KeyValuePair<int, string>> GetLocalizedStrings(string culture)
        {
            return null;
        }

        /// <summary>
        /// Retrieves all cultures supported by this device.
        /// See the <see cref="GetLocalizedStrings"/> method for more information
        /// about the format of each culture string.
        /// </summary>
        public virtual IEnumerable<string> GetSupportedCultures()
        {
            return null;
        }

        #endregion

        #region IFeedback Members

        /// <summary>
        /// The amount of time a device would take to report a new value.
        /// While we can poll for feedback at some interval, some devices may take some time
        /// until they report the new value. This value is in seconds.
        /// </summary>
        private const uint _defaultTimeUntilDeviceReportsNewValueMs = 20000;

        /// <summary>
        /// Specifies if the driver supports feedback from the device.
        /// <para />If this is false, then the driver will not provide any events for device feedback.
        /// <para />This value is specified by the JSON driver data - the following node must contain CommonFeatureSupport.SupportsFeedback and it must be set to true:
        /// <para />CrestronSerialDeviceApi.DeviceSupport
        /// </summary>
        public bool SupportsFeedback
        {
            get
            {
                return DriverData.CrestronSerialDeviceApi.DeviceSupport.ContainsKey(CommonFeatureSupport.SupportsFeedback)
                        && DriverData.CrestronSerialDeviceApi.DeviceSupport[CommonFeatureSupport.SupportsFeedback];
            }
        }

        /// <summary>
        /// Approximate minimum number of milliseconds the device or driver will report state/feedback changes.
        /// <para>
        /// The formula for this value is typically:
        /// [Polling Interval (if any)] + [Device Response Time] + [Response Processing Time].
        /// </para>
        /// <para>
        /// This may be 0 or a very small value if the device responds quickly
        /// and driver processing time is negligible.
        /// </para>
        /// <para>
        /// Amongst other reasons, this property can be used to write smarter code that waits at least this amount of time
        /// before processing changes to provide optimize updates to higher level components.
        /// </para>
        /// <para>
        /// If no capabilities of the device support feedback, this can be ignored.
        /// </para>
        /// </summary>
        public uint MinimumResponseTime
        {
            get
            {
                if (DriverData.CrestronSerialDeviceApi.Api.Exists() &&
                    DriverData.CrestronSerialDeviceApi.Api.Feedback.Exists())
                {
                    return DriverData.CrestronSerialDeviceApi.Api.Feedback.MinimumResponseTime;
                }

                // Let's estimate what this would be
                // Driver developers can specify the true value if needed
                // If someone has not initialized us, we will assume the default polling interval
                // and use that in the calculation.
                return DeviceProtocol.Exists() ?
                    (uint)DeviceProtocol.PollingInterval + _defaultTimeUntilDeviceReportsNewValueMs :
                    (uint)ABaseDriverProtocol.DefaultPollingIntervalInMs + _defaultTimeUntilDeviceReportsNewValueMs;
            }
        }

        /// <summary>
        /// Approximate maximum number of milliseconds the device or driver will take
        /// before reporting state/feedback changes after the device receives a command.
        /// <para>
        /// The formula for this value is typically:
        /// ([Device Response Time] + [Response Processing Time]) * 2 + [Polling Interval (if any)].
        /// </para>
        /// <para>
        /// The first part of the equation is multiplied by 2 because a command may be sent to the device
        /// just as the driver is beginning a poll.
        /// </para>
        /// <para>
        /// Amongst other reasons, this property can be used to write smarter code that waits at least this amount of time
        /// before timing out or declaring a device unresponsive.
        /// </para>
        /// <para>
        /// If no capabilities of the device support feedback, this can be ignored.
        /// </para>
        /// </summary>
        public uint MaximumResponseTime
        {
            get
            {
                if (DriverData.CrestronSerialDeviceApi.Api.Exists() &&
                    DriverData.CrestronSerialDeviceApi.Api.Feedback.Exists())
                {
                    return DriverData.CrestronSerialDeviceApi.Api.Feedback.MaximumResponseTime;
                }
                // Let's estimate what this would be
                // Driver developers can specify the true value if needed
                // If someone has not initialized us, we will assume the default polling interval
                // and use that in the calculation.

                // This is (PollingInterval * 2) + Average worst response time
                // It is multipled by two in case the control command is sent right as the polling 
                // sequence ends.
                return DeviceProtocol.Exists() ?
                    ((uint)DeviceProtocol.PollingInterval * 2) + _defaultTimeUntilDeviceReportsNewValueMs :
                    ((uint)ABaseDriverProtocol.DefaultPollingIntervalInMs * 2) + _defaultTimeUntilDeviceReportsNewValueMs;
            }
        }

        #endregion

        #region Fake Feedback

        /// <summary>
        /// Invoked whenever a command is sent to the device.
        /// </summary>
        /// <param name="command">CommandSet.StandardCommand</param>
        /// <param name="commandGroup">CommandSet.CommandGroup</param>
        private void OnFakeFeedbackChanged(StandardCommandsEnum command, CommonCommandGroupType commandGroup)
        {
            if (EnableLogging)
            {
                Log(string.Format("Attempting to fake feedback for standard command: {0}", command));
            }
        }

        /// <summary>
        /// Used by the framework to let device-types fake feedback based on the standard command and command group just sent to the transport.
        /// </summary>
        /// <param name="command">CommandSet.StandardCommand</param>
        /// <param name="commandGroup">CommandSet.CommandGroup</param>
        /// <returns>The device-type specific state object. Return null if this feautre should be circumvented</returns>
        protected virtual object FakeFeedbackForStandardCommand(StandardCommandsEnum command, CommonCommandGroupType commandGroup)
        {
            return null;
        }

        #endregion Fake Feedback
    }
}