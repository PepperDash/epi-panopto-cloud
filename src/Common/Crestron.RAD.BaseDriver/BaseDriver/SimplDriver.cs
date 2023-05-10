using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.RAD.Common.Transports;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.Reflection;
using Crestron.RAD.Common.Interfaces;
using System.Globalization;
using Crestron.SimplSharp.CrestronIO;
using Crestron.RAD.Common.Enums;

namespace Crestron.RAD.BaseDriver
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

    public abstract class SimplDriver<T>
    {
        private const int _logMaxCharacters = 150;

        protected BasicInformationDelegate InternalBasicInformationUpdated { get; set; }
        public TxUpdatedDelegate InternalTxUpdated { get; set; }
        public PacketTxUpdatedDelegate InternalPacketTxUpdated { get; set; }
        public LogDelegate InternalLogOut { get; set; }
        public RxOutDelegate InternalRxOut { get; set; }
        public BasicConnectionSupportsDelegate InternalBasicConnectionSupportsUpdated { get; set; }
        public CustomIrCommandSupportsDelegate InternalCustomIrCommandSupportsUpdatead { get; set; }
        public DriverLoadedDelegate InternalDriverLoadedCallback { get; set; }

        public string InterfaceName { get; set; }
        protected T Driver;
        private TransportType _transportType { get; set; }

        protected CTimer TimeOutTimer;
        private SimplTransport _simplTransport;
        private Action<string> _dataHandler;
        private string _lastMessageSent;
        private uint _timeOut;
        protected IrFileReader IrReader;
        protected bool UsingIr;
        protected bool EnableStackTrace;
        private uint _commandsSent;
        private readonly string PkgUnzipDirectory = "\\User\\RAD\\Drivers";
        private readonly string PkgUnzipTempDirectory = "\\User\\RAD\\Drivers\\Temp";
        private readonly string PkgLockFile = "\\User\\RAD\\PML.file";
        private CTimer _pkgLockTimer;
        private string _pkgFilename;
        private string _pkgIpAddress;
        private int _pkgPort;
        private int _pkgId;
        private bool _rebootRequired;
        private static CCriticalSection PkgCriticalSection = new CCriticalSection();

        public SimplDriver()
        {
            TimeOutTimer = new CTimer(CommandTimedOut, Timeout.Infinite);
            _pkgLockTimer = new CTimer(LoadPkgCallback, Timeout.Infinite);
            _commandsSent = 0;
            _rebootRequired = false;
        }

        public virtual void Initialize(string filename, int id, string ipAddress, int port)
        {
            if (InternalDriverLoadedCallback != null)
            {
                InternalDriverLoadedCallback(0, 0, _rebootRequired ? (ushort)1 : (ushort)0);
            }
            else
            {
                Log("SimplDriver - InternalDriverLoadedCallback callback missing in S+ code or it was not registered");
            }

            if (Driver != null)
            {
                ((IDisposable)Driver).Dispose();
                ClearStatus();
            }

            if (String.IsNullOrEmpty(filename) || !File.Exists(filename))
            {
                Log("SimplDriver - File not specified or does not exist");

                if (Driver != null)
                {
                    ((IDisposable)Driver).Dispose();
                }
                return;
            }

            if (filename.ToLower().EndsWith(".dll"))
            {
                LoadDllFile(filename, id, ipAddress, port);
            }
            else if (filename.ToLower().EndsWith(".ir"))
            {
                LoadIrFile(filename, port);
            }
            else if (filename.ToLower().EndsWith(".pkg"))
            {
                LoadPkgFile(filename, id, ipAddress, port);
            }
            else
            {
                if (InternalDriverLoadedCallback != null)
                {
                    InternalDriverLoadedCallback(0, 0, _rebootRequired ? (ushort)1 : (ushort)0);
                }

                Log("SimplDriver - Invalid file given. Must be a DLL, IR, or PKG file");
                return;
            }
        }

        private void LoadPkgFile(string filename, int id, string ipAddress, int port)
        {
            if (File.Exists(filename))
            {
                string lowerCaseDriverFilename = string.Empty;
                string driverDllFilename = string.Empty;
                string driverIrFilename = string.Empty;
                string driverDllTempFilename = string.Empty;
                string driverIrTempFilename = string.Empty;
                string driverDatFileName = string.Empty;

                try
                {
                    lowerCaseDriverFilename = Path.GetFileName(filename).ToLower();
                    driverDllFilename = Path.Combine(PkgUnzipDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".dll"));
                    driverIrFilename = Path.Combine(PkgUnzipDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".ir"));
                    driverDllTempFilename = Path.Combine(PkgUnzipTempDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".dll"));
                    driverIrTempFilename = Path.Combine(PkgUnzipTempDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".ir"));
                    driverDatFileName = Path.Combine(PkgUnzipTempDirectory, Path.ChangeExtension(lowerCaseDriverFilename, ".dat"));
                }
                catch (ArgumentException)
                {
                    Log(string.Format("SimplDriver - Pkg file loading failed - invalid filename provided ({0})", filename));
                    return;
                }

                bool isDllDriver = false;
                bool isIrDriver = false;

                // Wait here while other modules in other program slots load their PKG files
                if (File.Exists(PkgLockFile))
                {
                    _pkgFilename = filename;
                    _pkgId = id;
                    _pkgIpAddress = ipAddress;
                    _pkgPort = port;
                    _pkgLockTimer.Reset(1000, Timeout.Infinite);
                    return;
                }

                try
                {
                    PkgCriticalSection.Enter();

                    if (!Directory.Exists(PkgUnzipDirectory))
                    {
                        Directory.Create(PkgUnzipDirectory);
                    }
                    if (!Directory.Exists(PkgUnzipTempDirectory))
                    {
                        Directory.Create(PkgUnzipTempDirectory);
                    }

                    CrestronZIP.ResultCode unzipResultCode = CrestronZIP.ResultCode.ZR_FAILED;

                    (File.Create(PkgLockFile)).Close();
                    unzipResultCode = CrestronZIP.Unzip(filename, PkgUnzipTempDirectory);

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

                            // Delete temp files
                            File.Delete(driverDatFileName);
                            if (isDllDriver)
                            {
                                File.Delete(driverDllTempFilename);
                            }
                            else if (isIrDriver)
                            {
                                File.Delete(driverIrTempFilename);
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
                    InternalDriverLoadedCallback(1, (ushort)port, _rebootRequired ? (ushort)1 : (ushort)0);
                }
                finally
                {
                    if (File.Exists(PkgLockFile))
                    {
                        File.Delete(PkgLockFile);
                    }
                    PkgCriticalSection.Leave();

                    if (!_rebootRequired)
                    {
                        if (isDllDriver)
                        {
                            LoadDllFile(driverDllFilename, id, ipAddress, port);
                        }
                        else if (isIrDriver)
                        {
                            LoadIrFile(driverIrFilename, port);
                        }
                    }
                }
            }
            else
            {
                Log(string.Format("SimplDriver - File specified ({0}) does not exist on the control system", filename));
            }
        }

        private void DeleteFiles(string[] files)
        {
            foreach (string file in files)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception e)
                {
                    Log(string.Format("SimplDriver - Error removing file {0} - Reason={1}", file, e.Message));
                }
            }
        }

        private void LoadPkgCallback(object obj)
        {
            LoadPkgFile(_pkgFilename, _pkgId, _pkgIpAddress, _pkgPort);
        }

        private void LoadIrFile(string filename, int port)
        {
            if (Driver != null)
            {
                ((IDisposable)Driver).Dispose();
            }
            ClearStatus();

            if (port <= 0 && InternalLogOut != null)
            {
                InternalLogOut("SimplDriver - Port cannot be less than or equal to 0.");
                return;
            }

            var data = File.ReadToEnd(filename, Encoding.Default);
            IrReader = new IrFileReader { Port = port, SendTx = Send };
            IrReader.ProcessData(data);
            UsingIr = true;

            SetCommonDelegates();
            SetDeviceTypeSettings();
            SetSupportDelegates();

            if (InternalDriverLoadedCallback != null)
            {
                InternalDriverLoadedCallback(1, 0, _rebootRequired ? (ushort)1 : (ushort)0);
            }
        }

        private void LoadDllFile(string filename, int id, string ipAddress, int port)
        {
            UsingIr = false;
            var driverInfo = LoadDriver<T>(filename);

            if (driverInfo.Driver == null)
            {
                Log(String.Format("SimplDriver - No valid driver found in {0}", filename));
                return;
            }

            ((IBasicInformation)driverInfo.Driver).Id = (byte)id;

            if (driverInfo.TransportType == SimplDriver<T>.TransportType.None)
            {
                Log("SimplDriver - Driver contains no valid or supported transport interface.");
            }
            else
            {
                if (driverInfo.TransportType == SimplDriver<T>.TransportType.ISimpl)
                {
                    _simplTransport = ((ISimpl)driverInfo.Driver).Initialize(Send);
                    ((IBasicLogger)driverInfo.Driver).CustomLogger = SendLogOut;
                    _dataHandler = _simplTransport.ReceiveData;
                    _timeOut = _simplTransport.TimeOut;

                    _transportType = SimplDriver<T>.TransportType.ISimpl;
                }
                else if (driverInfo.TransportType == SimplDriver<T>.TransportType.ITcp)
                {
                    var address = IPAddress.Parse(ipAddress);

                    ((ITcp)driverInfo.Driver).Initialize(address, port != 0 ? port : ((ITcp)driverInfo.Driver).Port);
                    ((IBasicLogger)driverInfo.Driver).CustomLogger = SendLogOut;
                    _transportType = SimplDriver<T>.TransportType.ITcp;
                }
                else if (driverInfo.TransportType == SimplDriver<T>.TransportType.ICecDevice)
                {
                    _simplTransport = ((ICecDevice)driverInfo.Driver).Initialize(id, Send);
                    _dataHandler = _simplTransport.ReceiveData;

                    ((IBasicLogger)driverInfo.Driver).CustomLogger = SendLogOut;
                    _timeOut = _simplTransport.TimeOut;

                    _transportType = SimplDriver<T>.TransportType.ICecDevice;
                }

                Driver = driverInfo.Driver;

                SetCommonDelegates();
                SetDeviceTypeSettings();
                SetSupportDelegates();
                SetEventSubscriptions();

                if (InternalDriverLoadedCallback != null)
                {
                    InternalDriverLoadedCallback(1, (ushort)port, _rebootRequired ? (ushort)1 : (ushort)0);
                }

                ((IConnection)Driver).Connect();
            }
        }

        protected DriverInfo<T> LoadDriver<DriverType>(string fileName)
        {
            try
            {
                var dll = Assembly.LoadFrom(fileName);
                var types = dll.GetTypes();
                foreach (var cType in types)
                {
                    var interfaces = cType.GetInterfaces();

                    if (interfaces.FirstOrDefault(x => x.Name == InterfaceName) == null)
                    {
                        continue;
                    }

                    var simplDevice = interfaces.FirstOrDefault(x => x.Name == TransportType.ISimpl.ToString());
                    var tcpDevice = interfaces.FirstOrDefault(x => x.Name == TransportType.ITcp.ToString());
                    var cecDevice = interfaces.FirstOrDefault(x => x.Name == TransportType.ICecDevice.ToString());

                    DriverInfo<T> driverOutput = new DriverInfo<T>();

                    if (simplDevice != null)
                    {
                        driverOutput.Driver = (T)dll.CreateInstance(cType.FullName);
                        driverOutput.TransportType = TransportType.ISimpl;

                        return driverOutput;
                    }
                    else if (tcpDevice != null)
                    {
                        driverOutput.Driver = (T)dll.CreateInstance(cType.FullName);
                        driverOutput.TransportType = TransportType.ITcp;

                        return driverOutput;
                    }
                    else if (cecDevice != null)
                    {
                        driverOutput.Driver = (T)dll.CreateInstance(cType.FullName);
                        driverOutput.TransportType = TransportType.ICecDevice;

                        return driverOutput;
                    }
                }
            }
            catch (Exception e)
            {
                Log(String.Format("SimplDriver - failure occured while loading the driver. Reason={0}", e.Message));
                
                if (EnableStackTrace)
                {
                    Log(String.Format("SimplDriver: {0}", e.StackTrace));
                }
            }

            return new DriverInfo<T>();
        }

        private void SetCommonDelegates()
        {
            #region BasicInformation
            if (InternalBasicInformationUpdated != null)
            {
                if (UsingIr)
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
                else
                {
                    IBasicInformation basicInformation = (IBasicInformation)Driver;
                    
                    InternalBasicInformationUpdated(
                        new SimplSharpString(Common.Helpers.Constants.SdkVersion),
                        new SimplSharpString(basicInformation.Manufacturer),
                        new SimplSharpString(basicInformation.BaseModel),
                        new SimplSharpString(
                            basicInformation.SupportedModels.Count > 0 ?
                            basicInformation.SupportedModels.Aggregate((a, x) => a + ", " + x) :
                            basicInformation.BaseModel),
                        new SimplSharpString(string.Join(",", basicInformation.SupportedSeries.ToArray())),
                        new SimplSharpString(basicInformation.Description),
                        new SimplSharpString(basicInformation.DriverVersion),
                        new SimplSharpString(basicInformation.VersionDate.ToString(CultureInfo.InvariantCulture)),
                        new SimplSharpString(basicInformation.Guid.ToString()),
                        basicInformation.SupportsFeedback ? (ushort)1 : (ushort)0);
                }
            }
            #endregion

            #region Connection
            if (InternalBasicConnectionSupportsUpdated != null)
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

        #region Packet Transmission - COM Port
        public string SimplSetComportSpec(ushort comPort)
        {
            if (_transportType != SimplDriver<T>.TransportType.ISimpl)
            {
                return string.Empty;
            }

            string packetTransmissionInput = string.Empty;
            ushort cspec = 0;

            try
            {
                ISerialComport serialComport = (ISerialComport)Driver;

                if (serialComport != null)
                {
                    ComPortSpec comSpec = serialComport.ComSpec;

                    #region BaudRate
                    switch (comSpec.BaudRate)
                    {
                        case eComBaudRates.ComspecBaudRate300:
                            cspec |= 0x0000;
                            break;
                        case eComBaudRates.ComspecBaudRate600:
                            cspec |= 0x0001;
                            break;
                        case eComBaudRates.ComspecBaudRate1200:
                            cspec |= 0x0002;
                            break;
                        case eComBaudRates.ComspecBaudRate1800:
                            cspec |= 0x0080;
                            break;
                        case eComBaudRates.ComspecBaudRate2400:
                            cspec |= 0x0003;
                            break;
                        case eComBaudRates.ComspecBaudRate3600:
                            cspec |= 0x0081;
                            break;
                        case eComBaudRates.ComspecBaudRate4800:
                            cspec |= 0x0004;
                            break;
                        case eComBaudRates.ComspecBaudRate7200:
                            cspec |= 0x0082;
                            break;
                        case eComBaudRates.ComspecBaudRate9600:
                            cspec |= 0x0005;
                            break;
                        case eComBaudRates.ComspecBaudRate14400:
                            cspec |= 0x0083;
                            break;
                        case eComBaudRates.ComspecBaudRate19200:
                            cspec |= 0x0006;
                            break;
                        case eComBaudRates.ComspecBaudRate28800:
                            cspec |= 0x0084;
                            break;
                        case eComBaudRates.ComspecBaudRate38400:
                            cspec |= 0x0007;
                            break;
                        case eComBaudRates.ComspecBaudRate57600:
                            cspec |= 0x0085;
                            break;
                        case eComBaudRates.ComspecBaudRate115200:
                            cspec |= 0x0086;
                            break;
                        default:
                            cspec |= 0x0005;
                            break;
                    }
                    #endregion

                    #region Protocol
                    switch (comSpec.Protocol)
                    {
                        case eComProtocolType.ComspecProtocolRS232:
                            cspec |= 0x0000;
                            break;
                        case eComProtocolType.ComspecProtocolRS422:
                            cspec |= 0x0100;
                            break;
                        case eComProtocolType.ComspecProtocolRS485:
                            cspec |= 0x2100;
                            break;
                        default:
                            cspec |= 0x0000;
                            break;
                    }
                    #endregion

                    #region Parity
                    switch (comSpec.Parity)
                    {
                        case eComParityType.ComspecParityEven:
                            cspec |= 0x0008;
                            break;
                        case eComParityType.ComspecParityNone:
                            cspec |= 0x0000;
                            break;
                        case eComParityType.ComspecParityOdd:
                            cspec |= 0x0018;
                            break;
                        default:
                            cspec |= 0x0000;
                            break;
                    }
                    #endregion

                    #region DataBits
                    switch (comSpec.DataBits)
                    {
                        case eComDataBits.ComspecDataBits7:
                            cspec |= 0x0000;
                            break;
                        case eComDataBits.ComspecDataBits8:
                            cspec |= 0x0020;
                            break;
                        default:
                            cspec |= 0x0020;
                            break;
                    }
                    #endregion

                    #region StopBits
                    switch (comSpec.StopBits)
                    {
                        case eComStopBits.ComspecStopBits1:
                            cspec |= 0x0000;
                            break;
                        case eComStopBits.ComspecStopBits2:
                            cspec |= 0x0040;
                            break;
                        default:
                            cspec |= 0x0000;
                            break;
                    }
                    #endregion

                    #region SoftHandshake
                    switch (comSpec.SoftwareHandshake)
                    {
                        case eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone:
                            cspec |= 0x0000;
                            break;
                        case eComSoftwareHandshakeType.ComspecSoftwareHandshakeXON:
                            cspec |= 0x1800;
                            break;
                        case eComSoftwareHandshakeType.ComspecSoftwareHandshakeXONR:
                            cspec |= 0x0800;
                            break;
                        case eComSoftwareHandshakeType.ComspecSoftwareHandshakeXONT:
                            cspec |= 0x1000;
                            break;
                        default:
                            cspec |= 0x8000;
                            break;
                    }
                    #endregion

                    #region HardHandshake
                    switch (comSpec.HardwareHandShake)
                    {
                        case eComHardwareHandshakeType.ComspecHardwareHandshakeCTS:
                            cspec |= 0x0200;
                            break;
                        case eComHardwareHandshakeType.ComspecHardwareHandshakeRTS:
                            cspec |= 0x0400;
                            break;
                        case eComHardwareHandshakeType.ComspecHardwareHandshakeRTSCTS:
                            cspec |= (0x0400 | 0x0200);
                            break;
                        case eComHardwareHandshakeType.ComspecHardwareHandshakeNone:
                            cspec |= 0x0000;
                            break;
                        default:
                            cspec |= 0x0000;
                            break;
                    }
                    #endregion
                }

            }
            catch (Exception e)
            {
                Log(String.Format("SimplDriver - Error setting COM settings. Reason={0}", e.Message));

                if (EnableStackTrace)
                {
                    Log(String.Format("SimplDriver: {0}", e.StackTrace));
                }
            }

            return MakePacketTransmissionString(cspec, comPort);
        }

        private string MakePacketTransmissionString(ushort data, ushort port)
        {
            if (port >= 'A' && port <= 'F')
            {
                port = Convert.ToUInt16(0x80 | (port - 'A'));
            }
            else if (port >= '1' && port <= '6')
            {
                port = Convert.ToUInt16(0x80 | (port - '1'));
            }
            else if (port >= 1 && port <= 6)
            {
                port = port.ToString("X")[0];
                port = Convert.ToUInt16(0x80 | (port - '1'));
            }
            else
            {
                port = 0x80;
            }

            StringBuilder builder = new StringBuilder();

            builder.Append(Convert.ToChar(0x12));                   // ?
            builder.Append(Convert.ToChar(port));                   // Port
            builder.Append(Convert.ToChar(0x00));                   // ?
            builder.Append(Convert.ToChar((byte)(data & 0xFF)));    // Low bytes of data
            builder.Append(Convert.ToChar((byte)(data >> 8)));      // High byes of data
            builder.Append(Convert.ToChar(0x00));                   // Pacing
            builder.Append(Convert.ToChar(0x00));                   // ?

            return builder.ToString();
        }
        #endregion

        #region TX/RX

        public void Send(string message, object[] parameters)
        {
            _lastMessageSent = message;

            // Send the message to the Tx Signal / Packet Tx Signal 
            if (UsingIr)
            {
                if (InternalPacketTxUpdated != null)
                {
                    InternalPacketTxUpdated(message);
                }
            }
            else
            {
                if (InternalTxUpdated != null)
                {
                    InternalTxUpdated(message);
                }
                if (TimeOutTimer != null && _commandsSent < 1)
                {
                    _commandsSent++;
                    TimeOutTimer.Reset(_timeOut);
                }
            }
        }

        public void ReceiveData(string message)
        {
            _commandsSent = 0;

            if (TimeOutTimer != null)
            {
                TimeOutTimer.Stop();
            }

            if (_dataHandler != null)
            {
                _dataHandler(message);
            }
        }

        private void CommandTimedOut(object userspecific)
        {
            _commandsSent = 0;

            if (TimeOutTimer != null)
            {
                TimeOutTimer.Stop();
            }

            if (_simplTransport.MessageTimedOut != null)
            {
                _simplTransport.MessageTimedOut(_lastMessageSent);
            }
        }

        #endregion

        #region BasicLogger

        public void SimplEnableLogging()
        {
            if (Driver != null)
            {
                ((IBasicLogger)Driver).EnableLogging = true;
            }
        }

        public void SimplDisableLogging()
        {
            if (Driver != null)
            {
                ((IBasicLogger)Driver).EnableLogging = false;
            }
        }

        public void SimplEnableTxDebug()
        {
            if (Driver != null)
            {
                ((IBasicLogger)Driver).EnableTxDebug = true;
            }
        }

        public void SimplDisableTxDebug()
        {
            if (Driver != null)
            {
                ((IBasicLogger)Driver).EnableTxDebug = false;
            }
        }

        public void SimplEnableRxDebug()
        {
            if (Driver != null)
            {
                ((IBasicLogger)Driver).EnableRxDebug = true;
            }
        }

        public void SimplDisableRxDebug()
        {
            if (Driver != null)
            {
                ((IBasicLogger)Driver).EnableRxDebug = false;
            }
        }

        private void SendLogOut(string message)
        {
            if (InternalLogOut != null)
            {
                foreach (Match match in SplitMessage(message))
                {
                    var valueWithCR = match.Value + "\r";
                    InternalLogOut(valueWithCR);
                }

            }
        }

        private MatchCollection SplitMessage(string message)
        {
            return Regex.Matches(message, @"(.{1," + _logMaxCharacters + @"})(?:\s|$)");
        }



        #endregion

        #region Connection

        public void SimplDisconnect()
        {
            if (Driver != null)
            {
                ((IConnection)Driver).Disconnect();
            }
        }

        public void SimplReconnect()
        {
            if (Driver != null)
            {
                ((IConnection)Driver).Reconnect();
            }
        }

        public void SimplConnect()
        {
            if (Driver != null)
            {
                ((IConnection)Driver).Connect();
            }
        }

        #endregion

        #region CustomCommand

        public void SimplEnableRxOut()
        {
            if (Driver != null)
            {
                ((ICustomCommand)Driver).EnableRxOut = true;
            }
        }

        public void SimplDisableRxOut()
        {
            if (Driver != null)
            {
                ((ICustomCommand)Driver).EnableRxOut = false;
            }
        }

        protected void SendRxOut(string message)
        {
            if (InternalRxOut != null)
            {
                InternalRxOut(message);
            }
        }

        public void SimplSendCustomCommand(string message)
        {
            if (Driver != null)
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
            else if (Driver != null)
            {
                ((ICustomCommandCollection)Driver).SendCustomCommandByName(commandName);
            }
        }

        public void SimplTriggerCustomCommand(string commandName)
        {
            if (UsingIr)
            {
                IrReader.TriggerFunctionPress(commandName, true, 0);
            }
            else if (Driver != null)
            {
                ((ICustomCommandCollection)Driver).SendCustomCommandByName(commandName);
            }
        }

        public ushort SimplCustomCommandExists(string commandName)
        {
            if (UsingIr)
            {
                return IrReader.IsACommand(commandName) ? (ushort)1 : (ushort)0;
            }

            return (ushort)0;
        }

        #endregion

        protected void Log(string message)
        {
            if (InternalLogOut != null)
            {
                InternalLogOut(new SimplSharpString(message));
            }
        }

        protected abstract void SetDeviceTypeSettings();
        protected abstract void SetEventSubscriptions();
        protected abstract void SetSupportDelegates();
        protected abstract void ClearStatus();

        public class DriverInfo<DriverType>
        {
            public DriverInfo()
            {
                TransportType = TransportType.None;
            }

            public TransportType TransportType;
            public T Driver;
        }

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
    }
}