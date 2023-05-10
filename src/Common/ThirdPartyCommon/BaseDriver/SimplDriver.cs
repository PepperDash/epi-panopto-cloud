using System;
using System.Linq;
using System.Text;
using Crestron.Panopto.Common.Helpers;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Transports;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.Reflection;
using Crestron.Panopto.Common.Interfaces;
using System.Globalization;
using Crestron.SimplSharp.CrestronIO;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.ExtensionMethods;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;

namespace Crestron.Panopto.Common.BasicDriver
{
    public delegate void BasicInformationDelegate(
            SimplSharpString sdkVersion, SimplSharpString manufacturer, SimplSharpString baseModel, SimplSharpString supportedModels, SimplSharpString series,
            SimplSharpString description, SimplSharpString driverVersion, SimplSharpString driverVersionDate, SimplSharpString guid, ushort supportsFeedback);
    public delegate void TxUpdatedDelegate(SimplSharpString message);
    public delegate void PacketTxUpdatedDelegate(SimplSharpString message);
    public delegate void LogDelegate(SimplSharpString logMessage);
    public delegate void RxOutDelegate(SimplSharpString message);
    public delegate void BasicConnectionSupportsDelegate(ushort disconnect, ushort reconnect);
    public delegate void CustomIrCommandSupportsDelegate();
    public delegate void DriverLoadedDelegate(ushort loaded, ushort port, ushort rebootRequired);

    public delegate void DriverAuthenticationSupportsDelegate(
        ushort supportsUsername, ushort supportsPassword, ushort isRequired);

    public delegate void DriverAuthenticationUpdateDelegate(ushort authStatus);

    public delegate void DriverCustomCommandDelegate();

    public abstract class SimplDriver<T>
    {
        protected BasicInformationDelegate InternalBasicInformationUpdated { get; set; }
        public TxUpdatedDelegate InternalTxUpdated { get; set; }
        public PacketTxUpdatedDelegate InternalPacketTxUpdated { get; set; }
        public LogDelegate InternalLogOut { get; set; }
        public RxOutDelegate InternalRxOut { get; set; }
        public BasicConnectionSupportsDelegate InternalBasicConnectionSupportsUpdated { get; set; }
        public CustomIrCommandSupportsDelegate InternalCustomIrCommandSupportsUpdatead { get; set; }
        public DriverLoadedDelegate InternalDriverLoadedCallback { get; set; }

        public DriverAuthenticationSupportsDelegate InternalDriverAuthenticationSupportsCallback { get; set; }
        public DriverAuthenticationUpdateDelegate InternalDriverAuthenticationUpdateCallback { get; set; }

        public DriverCustomCommandDelegate InternalDriverCustomCommandCallback { get; set; }

        public ushort[] UserAttributeIsAvailable;
        public ushort[] UserAttributeDataTypes;
        public ushort[] UserAttributeTypes;
        public string[] UserAttributeLabels;
        public ushort[] UserAttributeIsPersistent;
        public ushort[] UserAttributeRequiredForConnection;
        public string[] UserAttributeDescriptions;

        #region Custom Command Delegate

        public List<string> SimplCustomCmds;
        public SimplSharpString[] SimplCustomCommandNames = new SimplSharpString[10];
        public ushort SimplNumPages;
        public ushort SimplNumCustomCommands;
        public ushort SimplCurrentPage;

        #endregion

        public string ApplicationDirectory { get { return Directory.GetApplicationDirectory(); } }
        public string InterfaceName { get; set; }

        protected T Driver;
        protected IrFileReader IrReader;
        protected bool UsingIr;
        protected bool EnableStackTrace;

        protected static Point SmallIconSize = new Point(72, 72);
        protected static Point MediumIconSize = new Point(144, 144);
        protected static Point LargeIconSize = new Point(288, 288);

        #region Private fields

        private const int _logMaxCharacters = 150;

        /// <summary>
        /// This value should match the number of attributes that are supported by the SIMPL module
        /// </summary>
        private const int _maxUserAttributesSupported = 10;
        private const int _timelineEventInterval = 500;

        private readonly string _pkgUnzipDirectory = string.Format("\\user\\CrestronCertifiedDrivers\\App{0}\\Drivers", InitialParametersClass.ApplicationNumber);
        private readonly string _pkgUnzipTempDirectoryTemplate = string.Format("\\user\\CrestronCertifiedDrivers\\App{0}\\Drivers\\", InitialParametersClass.ApplicationNumber);

        private TransportType _transportType { get; set; }
        private SimplTransport _simplTransport;
        private Action<string> _dataHandler;
        private string _lastMessageSent;
        private uint _timeOut;
        private bool _rebootRequired;
        private static CCriticalSection _pkgCriticalSection = new CCriticalSection();
        private string _pkgUnzipTempDirectory;
        private string[] _userAttributeIds;
        private bool _usingIrPkgFile;

        #endregion Private fields

        #region Constructor

        public SimplDriver()
        {
            _rebootRequired = false;

            _pkgUnzipDirectory = OsHelper.ConvertPathBasedOnOs(_pkgUnzipDirectory);
            _pkgUnzipTempDirectoryTemplate = OsHelper.ConvertPathBasedOnOs(_pkgUnzipTempDirectoryTemplate);

            _userAttributeIds = new string[_maxUserAttributesSupported];
            UserAttributeDataTypes = new ushort[_maxUserAttributesSupported];
            UserAttributeIsAvailable = new ushort[_maxUserAttributesSupported];
            UserAttributeTypes = new ushort[_maxUserAttributesSupported];
            UserAttributeLabels = new string[_maxUserAttributesSupported];
            UserAttributeIsPersistent = new ushort[_maxUserAttributesSupported];
            UserAttributeRequiredForConnection = new ushort[_maxUserAttributesSupported];
            UserAttributeDescriptions = new string[_maxUserAttributesSupported];
        }

        #endregion Constructor

        #region Initialization

        public void Initialize(string filename, int id, string ipAddress, int port)
        {
            Initialize(filename, id, ipAddress, port, 0);
        }

        public void Initialize(string filename, int id, string ipAddress, int port, int enableLogging)
        {
            try
            {
                try
                {
                    filename = OsHelper.ConvertPathBasedOnOs(filename);
                }
                catch (Exception e)
                {
                    Log(string.Format("Unknown problem occurred processing the filename. Filename:\r{0}\rException:\r{1}", filename, e.Message));
                }

                if (InternalDriverLoadedCallback.Exists())
                {
                    InternalDriverLoadedCallback(0, 0, _rebootRequired ? (ushort)1 : (ushort)0);
                }
                else
                {
                    Log("SimplDriver - InternalDriverLoadedCallback callback missing in S+ code or it was not registered");
                }

                if (Driver.Exists())
                {
                    ((IDisposable)Driver).Dispose();
                    ClearStatus();
                }

                if (String.IsNullOrEmpty(filename) || !File.Exists(filename))
                {
                    Log("SimplDriver - File not specified or does not exist");

                    if (Driver.Exists())
                    {
                        ((IDisposable)Driver).Dispose();
                    }
                }
                else
                {
                    if (filename.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadDllFile(filename, id, ipAddress, port, enableLogging.Equals(1));
                    }
                    else if (filename.EndsWith(".ir", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadIrFile(filename, port, enableLogging.Equals(1));
                    }
                    else if (filename.EndsWith(".pkg", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadPkgFile(filename, id, ipAddress, port, enableLogging.Equals(1));
                    }
                    else
                    {
                        if (InternalDriverLoadedCallback.Exists())
                        {
                            InternalDriverLoadedCallback(0, 0, _rebootRequired ? (ushort)1 : (ushort)0);
                        }

                        Log("SimplDriver - Invalid file given. Must be a DLL, IR, or PKG file");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Error initializing SimplDriver Message:{0}", ex.Message));
            }
        }

        private void SetCommonDelegates()
        {
            #region BasicInformation
            if (InternalBasicInformationUpdated.Exists())
            {
                if (UsingIr)
                {
                    if (_usingIrPkgFile == false)
                    {
                        InternalBasicInformationUpdated(
                             new SimplSharpString(string.Empty),
                             new SimplSharpString(IrReader.Manufacturer),
                             new SimplSharpString(IrReader.Model),
                             new SimplSharpString(string.Empty),     // SupportedModels
                             new SimplSharpString(string.Empty),     // Description
                             new SimplSharpString(string.Empty),     // Series
                             new SimplSharpString(string.Empty),     // Driver Version
                             new SimplSharpString(string.Empty),     // Driver version date
                             new SimplSharpString("{3FE4A431-E5C9-459E-85B2-8B79F13E7FB6}"),
                             0);
                    }
                }
                else
                {
                    IBasicInformation basicInformation = (IBasicInformation)Driver;
                    IFeedbackInformation feedbackInfromation = (IFeedbackInformation)Driver;

                    var supportedModels = basicInformation.SupportedModels.Count > 0 ?
                            basicInformation.SupportedModels.Aggregate((a, x) => a + ", " + x) :
                            basicInformation.BaseModel;

                    var supportedSeries = string.Join(",", basicInformation.SupportedSeries.ToArray());

                    supportedModels = supportedModels.Truncate(200);
                    supportedSeries = supportedSeries.Truncate(200);

                    int stopAt = basicInformation.DriverVersion.LastIndexOf(".");
                    string sdkVersion = "Unknown";
                    if (basicInformation.DriverVersion.Length > 0 && stopAt < basicInformation.DriverVersion.Length && stopAt > 0)
                    {
                        sdkVersion = basicInformation.DriverVersion.Substring(0, stopAt);
                    }
                    else
                    {
                        Log("Error retrieving SDK version from driver");
                    }

                    InternalBasicInformationUpdated(
                        new SimplSharpString(sdkVersion),
                        new SimplSharpString(basicInformation.Manufacturer),
                        new SimplSharpString(basicInformation.BaseModel),
                        new SimplSharpString(supportedModels),
                        new SimplSharpString(supportedSeries),
                        new SimplSharpString(basicInformation.Description),
                        new SimplSharpString(basicInformation.DriverVersion),
                        new SimplSharpString(basicInformation.VersionDate.ToString(CultureInfo.InvariantCulture)),
                        new SimplSharpString(basicInformation.Guid.ToString()),
                        feedbackInfromation.SupportsFeedback ? (ushort)1 : (ushort)0);
                }
            }
            #endregion

            #region Connection
            if (InternalBasicConnectionSupportsUpdated.Exists())
            {
                if (UsingIr)
                {
                    InternalBasicConnectionSupportsUpdated(0, 0);
                }
                else
                {
                    IConnection connection = (IConnection)Driver;
                    InternalBasicConnectionSupportsUpdated(connection.SupportsDisconnect ? (ushort)1 : (ushort)0,
                        connection.SupportsReconnect ? (ushort)1 : (ushort)0);
                }
            }
            #endregion
        }

        private void ProcessDatFileGeneralInformation(DatFileRootObject rootObject)
        {
            if (rootObject.Exists())
            {
                _usingIrPkgFile = true;

                if (InternalBasicInformationUpdated.Exists())
                {
                    var rootSupportedModels = string.Join(", ", rootObject.supportedModels.ToArray());
                    var rootSupportedSeries = string.Join(", ", rootObject.supportedSeries.ToArray());

                    InternalBasicInformationUpdated(rootObject.sdkVersion, rootObject.manufacturer, rootObject.baseModel,
                        rootSupportedModels, rootSupportedSeries, rootObject.description,
                        rootObject.driverVersion, rootObject.driverVersionDate, string.Empty, 0);
                }
            }
        }

        #endregion Initialization

        #region TX/RX

        public void Send(string message, object[] parameters)
        {
            _lastMessageSent = message;

            // Send the message to the Tx Signal / Packet Tx Signal 
            if (UsingIr)
            {
                if (InternalPacketTxUpdated.Exists())
                {
                    InternalPacketTxUpdated(message);
                }
            }
            else
            {
                if (InternalTxUpdated.Exists())
                {
                    InternalTxUpdated(message);
                }
            }
        }

        public void ReceiveData(string message)
        {
            if (_dataHandler.Exists())
            {
                _dataHandler(message);
            }
        }

        #endregion

        #region BasicLogger

        protected void Log(string message)
        {
            if (InternalLogOut.Exists())
            {
                InternalLogOut(new SimplSharpString(message));
            }
        }

        protected virtual void EnableDriverResourceLogging() { }
        protected virtual void DisableDriverResourceLogging() { }

        public void SimplEnableLogging()
        {
            if (UsingIr)
            {
                IrReader.EnableLogging = true;
            }
            else if (Driver.Exists())
            {
                ((IBasicLogger)Driver).EnableLogging = true;
                EnableDriverResourceLogging();
            }
        }

        public void SimplDisableLogging()
        {
            if (UsingIr)
            {
                IrReader.EnableLogging = false;
            }
            else if (Driver.Exists())
            {
                ((IBasicLogger)Driver).EnableLogging = false;
                DisableDriverResourceLogging();
            }
        }

        public void SimplEnableTxDebug()
        {
            if (Driver.Exists())
            {
                ((IBasicLogger)Driver).EnableTxDebug = true;
            }
        }

        public void SimplDisableTxDebug()
        {
            if (Driver.Exists())
            {
                ((IBasicLogger)Driver).EnableTxDebug = false;
            }
        }

        public void SimplEnableRxDebug()
        {
            if (Driver.Exists())
            {
                ((IBasicLogger)Driver).EnableRxDebug = true;
            }
        }

        public void SimplDisableRxDebug()
        {
            if (Driver.Exists())
            {
                ((IBasicLogger)Driver).EnableRxDebug = false;
            }
        }

        #endregion

        #region Connection

        public void SimplDisconnect()
        {
            if (Driver.Exists())
            {
                ((IConnection)Driver).Disconnect();
            }
        }

        public void SimplReconnect()
        {
            if (Driver.Exists())
            {
                ((IConnection)Driver).Reconnect();
            }
        }

        public void SimplConnect()
        {
            if (Driver.Exists())
            {
                ((IConnection)Driver).Connect();
            }
        }

        #endregion

        #region CustomCommand

        public void SimplEnableRxOut()
        {
            if (Driver.Exists())
            {
                ((ICustomCommand)Driver).EnableRxOut = true;
            }
        }

        public void SimplDisableRxOut()
        {
            if (Driver.Exists())
            {
                ((ICustomCommand)Driver).EnableRxOut = false;
            }
        }

        protected void SendRxOut(string message)
        {
            if (InternalRxOut.Exists())
            {
                InternalRxOut(message);
            }
        }

        public void SimplSendCustomCommand(string message)
        {
            if (Driver.Exists())
            {
                ((ICustomCommand)Driver).SendCustomCommand(message);
            }
        }

        #endregion

        #region CustomCommandCollection

        public void SimplSendCustomCommandByName(string commandName)
        {
            if (UsingIr)
            {
                IrReader.TriggerFunctionPress(commandName, true, 0);
            }
            else if (Driver.Exists())
            {
                ((ICustomCommandCollection)Driver).SendCustomCommandByName(commandName);
            }
        }

        public void SimplSendCustomCommandByValue(string value)
        {
            if (Driver.Exists())
            {
                ((ICustomCommandCollection)Driver).SendCustomCommandValue(value);
            }
        }

        public void SimplTriggerCustomCommand(string commandName)
        {
            if (UsingIr)
            {
                IrReader.TriggerFunctionPress(commandName, true, 0);
            }
            else if (Driver.Exists())
            {
                ((ICustomCommandCollection)Driver).SendCustomCommandByName(commandName);
            }
        }

        public void SimplTriggerCustomCommandPress(string commandName)
        {
            if (UsingIr)
            {
                IrReader.TriggerFunctionPress(commandName, false, 0);
            }
            else if (Driver.Exists())
            {
                ((ICustomCommandCollection2)Driver).SendCustomCommandByName(commandName, CommandAction.Hold);
            }
        }

        public void SimplTriggerCustomCommandRelease(string commandName)
        {
            if (UsingIr)
            {
                IrReader.TriggerFunctionRelease();
            }
            else if (Driver.Exists())
            {
                ((ICustomCommandCollection2)Driver).SendCustomCommandByName(commandName, CommandAction.Release);
            }
        }

        public ushort SimplCustomCommandExists(string commandName)
        {
            if (UsingIr)
            {
                return IrReader.IsACommand(commandName) ? (ushort)1 : (ushort)0;
            }
            else if (Driver.Exists())
            {
                ushort @ushort = ((ICustomCommandCollection)Driver).CheckIfCustomCommandExists(commandName) ? (ushort)1 : (ushort)0;
                return @ushort;
            }
            return 0;
        }

        public void SimplCustomCommandPageUp()
        {
            ushort pageOffset = 0;
            ushort totalIndexesToOutput = 0;

            if (SimplCustomCmds != null)
            {
                if (SimplNumPages > 1 && SimplCurrentPage < SimplNumPages)
                {
                    SimplCurrentPage++;
                    pageOffset = (ushort)((SimplCurrentPage - 1) * 10);
                    totalIndexesToOutput = (ushort)(SimplNumCustomCommands - pageOffset);
                    if (totalIndexesToOutput > 10)
                        totalIndexesToOutput = 10;

                    //Clear the output array

                    for (int i = 0; i < 10; i++)
                    {
                        SimplCustomCommandNames[i] = "-";
                    }

                    //populate the output array

                    if (totalIndexesToOutput > 0)
                    {
                        for (int j = 0; j < totalIndexesToOutput; j++)
                        {
                            SimplCustomCommandNames[j] = SimplCustomCmds[j + pageOffset];
                        }
                    }
                    UpdateCustomCommands(SimplCustomCommandNames, SimplCurrentPage);
                }
            }
        }

        public void SimplCustomCommandPageDown()
        {
            ushort pageOffset = 0;
            ushort totalIndexesToOutput = 0;

            if (SimplCustomCmds != null)
            {
                if (SimplNumPages > 1 && SimplCurrentPage > 1)
                {
                    SimplCurrentPage--;
                    pageOffset = (ushort)((SimplCurrentPage - 1) * 10);
                    totalIndexesToOutput = (ushort)(SimplNumCustomCommands - pageOffset);
                    if (totalIndexesToOutput > 10)
                        totalIndexesToOutput = 10;

                    //Clear the output array

                    for (int i = 0; i < 10; i++)
                    {
                        SimplCustomCommandNames[i] = "-";
                    }

                    //populate the output array
                    if (totalIndexesToOutput > 0)
                    {
                        for (int j = 0; j < totalIndexesToOutput; j++)
                        {
                            SimplCustomCommandNames[j] = SimplCustomCmds[j + pageOffset];
                        }
                    }
                    UpdateCustomCommands(SimplCustomCommandNames, SimplCurrentPage);
                }
            }
        }

        public void SetDriverCustomCommands()
        {
             if (Driver.Exists())
            {
                if (((ICustomCommandCollection)Driver).CustomCommandNames.Exists())
                {
                    try
                    {
                        SimplCustomCmds = new List<string>();
                        SimplCustomCmds = ((ICustomCommandCollection)Driver).CustomCommandNames.ToList();

                        SimplNumCustomCommands = (ushort)SimplCustomCmds.Count();
                        SimplNumPages = CalculateCustomCmdNumPages(SimplNumCustomCommands);
                    }
                    catch (Exception e)
                    {
                        Log(string.Format("SimplDriver - Exception Assigning Simpl Custom Command Fields: {0}", e.Message));
                    }

                    if (SimplNumPages > 0)
                    {
                        SimplCurrentPage = 1;
                    }
                    ushort totalIndexesToOutput = SimplNumCustomCommands;
                    if (totalIndexesToOutput > 10)
                        totalIndexesToOutput = 10;

                    //clear the output array

                    for (int i = 0; i < 10; i++)
                    {
                        SimplCustomCommandNames[i] = "-";
                    }

                    //populate the output array

                    if (totalIndexesToOutput > 0)
                    {
                        for (int j = 0; j < totalIndexesToOutput; j++)
                        {
                            SimplCustomCommandNames[j] = SimplCustomCmds[j];
                        }
                    }
                    UpdateCustomCommands(SimplCustomCommandNames, SimplCurrentPage);
                }
                else
                {
                    Log("Simpl Driver - No custom commands in driver");
                }
            }
        }

        private ushort CalculateCustomCmdNumPages(ushort num)
        {
            ushort value = 0;
            value = (ushort)(num / 10);
            if ((num % 10) > 0)
                value += 1;

            return value;
        }

        public void UpdateCustomCommands(SimplSharpString[] labels, ushort curPage)
        {
            try
            {
                if (Driver.Exists())
                {
                    if (InternalDriverCustomCommandCallback.Exists() && labels[0] != (SimplSharpString) "-")
                    {
                        InternalDriverCustomCommandCallback();
                    }
                }

            }
            catch (Exception e)
            {
                Log(string.Format("Exception occurred when updating SimplAVReceiver Simpl Custom Commands in s+: {0}", e.Message));
            }
        }

        #endregion

        #region User Attributes

        public void SimplSetStringUserAttribute(ushort index, string value)
        {
            if (Driver.Exists())
            {
                ((IBasicInformation2)Driver).SetUserAttribute(_userAttributeIds[index - 1], value);
            }
        }

        public void SimplSetAnalogUserAttribute(ushort index, ushort value)
        {
            if (Driver.Exists())
            {
                ((IBasicInformation2)Driver).SetUserAttribute(_userAttributeIds[index - 1], value);
            }
        }

        public void SimplSetDigitalUserAttribute(ushort index, ushort value)
        {
            if (Driver.Exists())
            {
                ((IBasicInformation2)Driver).SetUserAttribute(_userAttributeIds[index - 1], value == 1 ? true : false);
            }
        }

        private void PopulateUserAttributes()
        {
            if (Driver.Exists())
            {
                _userAttributeIds = new string[_maxUserAttributesSupported];
                UserAttributeDataTypes = new ushort[_maxUserAttributesSupported];
                UserAttributeIsAvailable = new ushort[_maxUserAttributesSupported];
                UserAttributeTypes = new ushort[_maxUserAttributesSupported];
                UserAttributeLabels = new string[_maxUserAttributesSupported];
                UserAttributeIsPersistent = new ushort[_maxUserAttributesSupported];
                UserAttributeRequiredForConnection = new ushort[_maxUserAttributesSupported];
                UserAttributeDescriptions = new string[_maxUserAttributesSupported];

                var attributes = ((IBasicInformation2)Driver).RetrieveUserAttributes();
                if (attributes.Exists())
                {
                    // Only loop through the user attributes that could show up in the SIMPL module
                    var attributeCount = attributes.Count > _maxUserAttributesSupported ? _maxUserAttributesSupported : attributes.Count;
                    for (int i = 0; i < attributeCount; i++)
                    {
                        _userAttributeIds[i] = attributes[i].ParameterId;
                        UserAttributeLabels[i] = attributes[i].Label;
                        UserAttributeDescriptions[i] = attributes[i].Description;
                        UserAttributeIsAvailable[i] = 1;
                        UserAttributeIsPersistent[i] = attributes[i].Persistent ? (ushort)1 : (ushort)0;

                        if (attributes[i].TypeName.Exists())
                        {
                            if (attributes[i].TypeName.Equals("deviceid", StringComparison.OrdinalIgnoreCase))
                            {
                                UserAttributeTypes[i] = (ushort)UserAttributeType.DeviceId;
                            }
                            else if (attributes[i].TypeName.Equals("onscreenid", StringComparison.OrdinalIgnoreCase))
                            {
                                UserAttributeTypes[i] = (ushort)UserAttributeType.OnScreenId;
                            }
                            else if (attributes[i].TypeName.Equals("messagebox", StringComparison.OrdinalIgnoreCase))
                            {
                                UserAttributeTypes[i] = (ushort)UserAttributeType.MessageBox;
                            }
                            else if (attributes[i].TypeName.Equals("custom", StringComparison.OrdinalIgnoreCase))
                            {
                                UserAttributeTypes[i] = (ushort)UserAttributeType.Custom;
                            }

                        }

                        if (attributes[i].Data.Exists() &&
                            attributes[i].Data.DataType.Exists())
                        {
                            if (attributes[i].Data.DataType.Equals("string", StringComparison.OrdinalIgnoreCase))
                            {
                                UserAttributeDataTypes[i] = (ushort)UserAttributeDataType.String;
                            }
                            else if (attributes[i].Data.DataType.Equals("number", StringComparison.OrdinalIgnoreCase))
                            {
                                UserAttributeDataTypes[i] = (ushort)UserAttributeDataType.Number;
                            }
                            else if (attributes[i].Data.DataType.Equals("boolean", StringComparison.OrdinalIgnoreCase))
                            {
                                UserAttributeDataTypes[i] = (ushort)UserAttributeDataType.Boolean;
                            }
                            else if (attributes[i].Data.DataType.Equals("hex", StringComparison.OrdinalIgnoreCase))
                            {
                                UserAttributeDataTypes[i] = (ushort)UserAttributeDataType.Hex;
                            }

                        }

                        if (attributes[i].RequiredForConnection.Exists())
                        {
                            if (String.Compare(attributes[i].RequiredForConnection, "none", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                UserAttributeRequiredForConnection[i] = (ushort)UserAttributeRequiredForConnectionType.None;
                            }
                            else if (String.Compare(attributes[i].RequiredForConnection, "before", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                UserAttributeRequiredForConnection[i] = (ushort)UserAttributeRequiredForConnectionType.Before;
                            }
                            else if (String.Compare(attributes[i].RequiredForConnection, "after", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                UserAttributeRequiredForConnection[i] = (ushort)UserAttributeRequiredForConnectionType.After;
                            }
                        }
                    }
                }
            }
        }

        #endregion User Attributes

        #region Authentication

        private void SetDriverAuthentication()
        {
            if (InternalDriverAuthenticationSupportsCallback.Exists())
            {
                if (UsingIr)
                {
                    InternalDriverAuthenticationSupportsCallback(0, 0, 0);
                }
                else if (Driver.Exists())
                {
                    var authentication = (IAuthentication)Driver;
                    var authIsRequired = (IAuthentication2)Driver;
                    InternalDriverAuthenticationSupportsCallback(
                        authentication.SupportsUsername ? (ushort)1 : (ushort)0,
                        authentication.SupportsPassword ? (ushort)1 : (ushort)0,
                        authIsRequired.Required ? (ushort)1 : (ushort)0);
                }
            }
        }

        #endregion Authentication

        #region Driver Loading

        protected DriverInfo<T> LoadDriver<DriverType>(string fileName)
        {
            //update LoadDriverTest unit test if modifying this method

            var driverInfo = new DriverInfo<T>();
            try
            {
                var dll = Assembly.LoadFrom(fileName);
                CType[] types = dll.GetTypes();

                for (int onType = 0; onType < types.Length; onType++)
                {
                    CType cType = types[onType];
                    var interfaces = cType.GetInterfaces();

                    var simplDevice = interfaces.FirstOrDefault(x => x.Name.Equals(TransportType.ISimpl.ToString()));
                    var tcpDevice = interfaces.FirstOrDefault(x => x.Name.Equals(TransportType.ITcp.ToString()));
                    var cecDevice = interfaces.FirstOrDefault(x => x.Name.Equals(TransportType.ICecDevice.ToString()));

                    if (simplDevice.Exists())
                    {
                        driverInfo.Driver = (T)dll.CreateInstance(cType.FullName);
                        driverInfo.TransportType = TransportType.ISimpl;
                        break;
                    }
                    else if (tcpDevice.Exists())
                    {
                        driverInfo.Driver = (T)dll.CreateInstance(cType.FullName);
                        driverInfo.TransportType = TransportType.ITcp;
                        break;
                    }
                    else if (cecDevice.Exists())
                    {
                        driverInfo.Driver = (T)dll.CreateInstance(cType.FullName);
                        driverInfo.TransportType = TransportType.ICecDevice;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                //CrestronConsole.PrintLine("SimplDriver - Failure: {0}", e);
                Log(String.Format("SimplDriver - failure occured while loading the driver. Reason={0}", e.ToString()));
                
                if (EnableStackTrace)
                {
                    Log(String.Format("SimplDriver: {0}", e.StackTrace));
                }
            }

            return driverInfo;
        }

        private void LoadPkgFile(string filename, int id, string ipAddress, int port, bool enableLogging)
        {
            if (File.Exists(filename) == false)
            {
                Log(string.Format("SimplDriver - File specified ({0}) does not exist on the control system", filename));
                return;
            }
            //_pkgUnzipTempDirectory = string.Format("{0}\\{1}", _pkgUnzipTempDirectoryTemplate, Guid.NewGuid().ToString());
            _pkgUnzipTempDirectory = Path.Combine(_pkgUnzipTempDirectoryTemplate, Guid.NewGuid().ToString());
            string lowerCaseDriverFilename = string.Empty;
            string driverDllFilename = string.Empty;
            string driverIrFilename = string.Empty;
            string driverDllTempFilename = string.Empty;
            string driverIrTempFilename = string.Empty;
            string driverDatFileName = string.Empty;

            try
            {
                lowerCaseDriverFilename = Path.GetFileName(filename);
                driverDllFilename = Path.Combine(_pkgUnzipDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".dll"));
                driverIrFilename = Path.Combine(_pkgUnzipDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".ir"));
                driverDllTempFilename = Path.Combine(_pkgUnzipTempDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".dll"));
                driverIrTempFilename = Path.Combine(_pkgUnzipTempDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".ir"));
                driverDatFileName = Path.Combine(_pkgUnzipTempDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".dat"));
            }
            catch (ArgumentException)
            {
                Log(string.Format("SimplDriver - Pkg file loading failed - invalid filename provided ({0})", filename));
                return;
            }

            bool isDllDriver = false;
            bool isIrDriver = false;

            try
            {
                _pkgCriticalSection.Enter();

                if (!Directory.Exists(_pkgUnzipDirectory))
                {
                    Directory.Create(_pkgUnzipDirectory);
                }
                if (!Directory.Exists(_pkgUnzipTempDirectory))
                {
                    Directory.Create(_pkgUnzipTempDirectory);
                }

                CrestronZIP.ResultCode unzipResultCode = CrestronZIP.ResultCode.ZR_FAILED;

                unzipResultCode = CrestronZIP.Unzip(filename, _pkgUnzipTempDirectory);

                if (unzipResultCode == CrestronZIP.ResultCode.ZR_OK)
                {
                    // Verify a valid PKG file was unzipped - should contain a .DAT file and .DLL/.IR file
                    if (!File.Exists(driverDatFileName) || (!File.Exists(driverDllTempFilename) && !File.Exists(driverIrTempFilename)))
                    {
                        Log("SimplDriver - The PKG file is missing files");
                    }
                    else
                    {
                        isDllDriver = File.Exists(driverDllTempFilename);
                        isIrDriver = File.Exists(driverIrTempFilename);

                        // Compare extracted driver to any pre-existing ones with the same name
                        if (isDllDriver && File.Exists(driverDllFilename))
                        {
                            if (File.GetLastWriteTime(driverDllFilename) != File.GetLastWriteTime(driverDllTempFilename))
                            {
                                File.Delete(driverDllFilename);
                                File.Move(driverDllTempFilename, driverDllFilename);
                            }
                        }
                        else if (isIrDriver && File.Exists(driverIrFilename))
                        {
                            if (File.GetLastWriteTime(driverIrFilename) != File.GetLastWriteTime(driverIrTempFilename))
                            {
                                File.Delete(driverIrFilename);
                                File.Move(driverIrTempFilename, driverIrFilename);
                            }
                        }
                        else
                        {
                            if (isDllDriver)
                            {
                                File.Move(driverDllTempFilename, driverDllFilename);
                            }
                            else if (isIrDriver)
                            {
                                File.Delete(driverIrFilename);
                                File.Move(driverIrTempFilename, driverIrFilename);
                            }
                        }
                    }
                }
                else
                {
                    Log(string.Format("SimplDriver - Error unzipping PKG file - Error={0}", unzipResultCode));
                }
            }
            catch (Exception)
            {
                Log("SimplDriver - Reboot required");
                _rebootRequired = true;
                if (InternalDriverLoadedCallback.Exists())
                {
                    InternalDriverLoadedCallback(1, (ushort)port, _rebootRequired ? (ushort)1 : (ushort)0);
                }
            }
            finally
            {
                _pkgCriticalSection.Leave();
            }

            if (!_rebootRequired)
            {
                try
                {
                    if (isDllDriver)
                    {
                        LoadDllFile(driverDllFilename, id, ipAddress, port, enableLogging);
                    }
                    else if (isIrDriver)
                    {
                        ProcessDatFileSettings(driverDatFileName);
                        LoadIrFile(driverIrFilename, port, enableLogging);
                    }
                    DeletePkgTempDirectory(_pkgUnzipTempDirectory);
                }
                catch (Exception e)
                {
                    Log(string.Format("SimplDriver - Error loading driver - Error={0}\r\nStack Trace: {1}", e.Message, e.StackTrace));
                }
            }
        }

        private void DeletePkgTempDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            catch
            { }
        }

        private void LoadIrFile(string filename, int port, bool enableLogging)
        {
            if (Driver.Exists())
            {
                ((IDisposable)Driver).Dispose();
            }
            ClearStatus();

            if (port <= 0 && InternalLogOut.Exists())
            {
                InternalLogOut("SimplDriver - Port cannot be less than or equal to 0.");
                return;
            }

            IrReader = new IrFileReader { Port = port, SendTx = Send, Logger = CrestronConsole.PrintLine, EnableLogging = enableLogging };
            IrReader.LoadIrFile(filename);
            UsingIr = true;

            SetCommonDelegates();
            SetDeviceTypeSettings();
            SetSupportDelegates();
            SetDriverAuthentication();

            if (InternalDriverLoadedCallback.Exists())
            {
                InternalDriverLoadedCallback(1, 0, _rebootRequired ? (ushort)1 : (ushort)0);
            }
        }

        private void LoadDllFile(string filename, int id, string ipAddress, int port, bool enableLogging)
        {
            UsingIr = false;
            var driverInfo = LoadDriver<T>(filename);

            if (driverInfo.Driver.DoesNotExist())
            {
                Log(String.Format("SimplDriver - No valid driver found in {0}", filename));
            }
            else
            {
                ((IBasicInformation)driverInfo.Driver).Id = (byte)id;

                if (driverInfo.TransportType == SimplDriver<T>.TransportType.None)
                {
                    Log("SimplDriver - Driver contains no valid or supported transport interface.");
                }
                else
                {
                    var driverInitializeWasSuccessful = false;

                    ((IBasicLogger)driverInfo.Driver).EnableLogging = enableLogging;

                    switch (driverInfo.TransportType)
                    {
                        case SimplDriver<T>.TransportType.ISimpl:
                            driverInitializeWasSuccessful = SetupComDriver(driverInfo);
                            break;
                        case SimplDriver<T>.TransportType.ITcp:
                            driverInitializeWasSuccessful = SetupTcpDriver(driverInfo, ipAddress, port);
                            break;
                        default:
                            Log("Driver did not contain valid transport");
                            return;
                    }

                    if (driverInitializeWasSuccessful)
                    {
                        Driver = driverInfo.Driver;

                        PopulateUserAttributes();
                        SetCommonDelegates();
                        SetDeviceTypeSettings();
                        SetSupportDelegates();
                        SetDriverAuthentication();
                        SetEventSubscriptions();

                        if (InternalDriverCustomCommandCallback.Exists())
                        {
                            SetDriverCustomCommands();
                        }

                        if (InternalDriverLoadedCallback.Exists())
                        {
                            InternalDriverLoadedCallback(1, (ushort)port, _rebootRequired ? (ushort)1 : (ushort)0);
                        }

                        ((IConnection)Driver).Connect();
                    }
                    else
                    {
                        Log("SimplDriver - Driver failed to initialize");
                    }
                }
            }
        }

        private bool SetupTcpDriver(DriverInfo<T> driverInfo, string ipAddress, int port)
        {
            var ableToInitialize = false;

            if (string.IsNullOrEmpty(ipAddress))
            {
                Log("SimplDriver - No IP address provided");
            }
            else
            {
                var address = IPAddress.None;

                try
                {
                    address = IPAddress.Parse(ipAddress);
                }
                catch (Exception e)
                {
                    Log(string.Format("SimplDriver - Unable to parse provided IP Address. Value = {0} Exception = {1}", ipAddress, e.Message));
                }
                try
                {
                    if (!address.Equals(IPAddress.None))
                    {
                        ((ITcp)driverInfo.Driver).Initialize(address, port != 0 ? port : ((ITcp)driverInfo.Driver).Port);
                        _transportType = SimplDriver<T>.TransportType.ITcp;

                        ableToInitialize = true;
                    }
                }
                catch (Exception e)
                {
                    Log(string.Format("SimplDriver - Unable to initialize TCP driver. Exception = {0}", e.Message));
                }
            }

            return ableToInitialize;
        }

        private bool SetupComDriver(DriverInfo<T> driverInfo)
        {
            var ableToInitialize = false;

            try
            {
                _simplTransport = ((ISimpl)driverInfo.Driver).Initialize(Send);
                _dataHandler = _simplTransport.ReceiveData;
                _timeOut = _simplTransport.TimeOut;
                _transportType = SimplDriver<T>.TransportType.ISimpl;

                ableToInitialize = true;
            }
            catch (Exception e)
            {
                Log(string.Format("SimplDriver - Unable to initialize COM driver. Exception = {0}", e.Message));
            }

            return ableToInitialize;
        }

        #endregion Driver Loading

        #region DAT File Processing

        private void ProcessDatFileSettings(string datFilename)
        {
            if (File.Exists(datFilename) &&
               Path.GetExtension(datFilename).Equals(".dat", StringComparison.InvariantCultureIgnoreCase))
            {
                string datFileJsonString = string.Empty;
                using (StreamReader reader = new StreamReader(datFilename))
                {
                    datFileJsonString = reader.ReadToEnd();
                }

                try
                {
                    var authenticationNodeConverter = new DatACommunicationJsonConverter();
                    var stringToEnum = new StringEnumConverter();
                    var serializerSettings = new JsonSerializerSettings { Converters = { authenticationNodeConverter, stringToEnum } };

                    var rootObject = JsonConvert.DeserializeObject<DatFileRootObject>(datFileJsonString, serializerSettings);
                    if (rootObject.Exists())
                    {
                        ProcessDatFileGeneralInformation(rootObject);
                        if (rootObject.power.Exists())
                        {
                            ProcessDatFilePowerSettings(rootObject.power.WarmUpTime, rootObject.power.CoolDownTime);
                        }
                        else
                        {
                            Log("No power settings found in DAT file");
                        }

                        if (rootObject.multiPowerOff.Exists())
                        {
                            ProcessDatFileMultiPowerOffSettings(rootObject.multiPowerOff.commands, rootObject.multiPowerOff.timeBetweenInSeconds);
                        }
                        else
                        {
                            Log("Multiple power off commands are not supported or defined by this driver");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(string.Format("Exception occured while reading a DAT file for an IR PKG file - {0}", e.Message));
                }
            }
        }

        protected virtual void ProcessDatFileMultiPowerOffSettings(List<string> commands, float timeBetweenInSeconds)
        {
            // No base implementation
        }

        protected virtual void ProcessDatFilePowerSettings(uint warmUpTime, uint coolDownTime)
        {
            // No base implementation
        }

        #endregion DAT File Processing

        #region Classes

        public class DriverInfo<DriverType>
        {
            public DriverInfo()
            {
                TransportType = TransportType.None;
            }

            public TransportType TransportType;
            public T Driver;
        }

        #endregion Classes

        #region Enumerables

        public enum TransportType
        {
            None = 0,
            ICecDevice = 1,
            ISerialComport = 2,
            ITcp = 3,
            Http = 4,
            Telnet = 5,
            ISimpl = 6,
            ICrestronConnected = 7,
            IIr = 8
        }

        #endregion Enumerables

        #region Abstract methods

        protected abstract void SetDeviceTypeSettings();
        protected abstract void SetEventSubscriptions();
        protected abstract void SetSupportDelegates();
        protected abstract void ClearStatus();

        #endregion Abstract methods
    }
}