// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Transports;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using eStringEncoding = Crestron.RAD.Common.Enums.eStringEncoding;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpProInternal;

namespace Crestron.RAD.ProTransports
{
    public class SerialTransport : IComPort
    {
        private IComPortDevice _port;

        public SerialTransport(ComPort port)
        {
            if (port == null) throw new NullReferenceException("Comport can not be null");
            
            _port = port;
            
            _port.SerialDataReceived += PortOnSerialDataReceived;
            
        }

        public SerialTransport(IComPortDevice port)
        {
            if (port == null) throw new NullReferenceException("IComPortDevice can not be null");

            _port = port;

            _port.SerialDataReceived += PortOnSerialDataReceived;
        }

        private void PortOnSerialDataReceived(IComPortDevice device, ComPortSerialDataEventArgs args)
        {
            if (_serialDataReceived == null) return;
            this.RcvdString = args.SerialData;
            _serialDataReceived(this, args.SerialData, (eStringEncoding)args.SerialEncoding);
        }

        public void Send(string dataToTransmit)
        {
            try
            {
                this.TransmitString = dataToTransmit;
                _port.Send(dataToTransmit);
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Exception occured while transmitting data to the COM port. Exception: {0}", e.ToString());
            }
        }

        public int SetComPortSpec(eComBaudRates paramBaudRate, eComDataBits paramNumberOfDataBits, eComParityType paramParityType,
            eComStopBits paramNumberOfStopBits, eComProtocolType paramProtocolType,
            eComHardwareHandshakeType paramHardwareHandShake, eComSoftwareHandshakeType paramSoftwareHandshake,
            bool paramReportCTSChanges)
        {
            _port.SetComPortSpec
                 (new ComPort.ComPortSpec
                    {
                        BaudRate = (ComPort.eComBaudRates)paramBaudRate,
                        DataBits = (ComPort.eComDataBits)paramNumberOfDataBits,
                        Parity = (ComPort.eComParityType)paramParityType,
                        StopBits = (ComPort.eComStopBits)paramNumberOfStopBits,
                        Protocol = (ComPort.eComProtocolType)paramProtocolType,
                        HardwareHandShake = (ComPort.eComHardwareHandshakeType)paramHardwareHandShake,
                        SoftwareHandshake = (ComPort.eComSoftwareHandshakeType)paramSoftwareHandshake,
                        ReportCTSChanges = paramReportCTSChanges
                    });
                    /*(ComPort.eComBaudRates)paramBaudRate,
                    (ComPort.eComDataBits)paramNumberOfDataBits,
                    (ComPort.eComParityType)paramParityType,
                    (ComPort.eComStopBits)paramNumberOfStopBits,
                    (ComPort.eComProtocolType)paramProtocolType,
                    (ComPort.eComHardwareHandshakeType)paramHardwareHandShake,
                    (ComPort.eComSoftwareHandshakeType)paramSoftwareHandshake,
                    paramReportCTSChanges*/

            return (int)_port.ID;

        }

        public int SetComPortSpec(ComPortSpec spec)
        {
            var specPro = new ComPort.ComPortSpec
            {
                BaudRate = (ComPort.eComBaudRates)spec.BaudRate,
                DataBits = (ComPort.eComDataBits)spec.DataBits,
                Parity = (ComPort.eComParityType)spec.Parity,
                StopBits = (ComPort.eComStopBits)spec.StopBits,
                Protocol = (ComPort.eComProtocolType)spec.Protocol,
                HardwareHandShake = (ComPort.eComHardwareHandshakeType)spec.HardwareHandShake,
                SoftwareHandshake = (ComPort.eComSoftwareHandshakeType)spec.SoftwareHandshake,
                ReportCTSChanges = spec.ReportCTSChanges
            };

            _port.SetComPortSpec(specPro);
            return (int) _port.ID;
        }

        public bool UnRegister()
        {
            if (_port is PortDevice)
            {
                return (_port as PortDevice).UnRegister() == eDeviceRegistrationUnRegistrationResponse.Success;    
            }
            return false;

        }

        public string RcvdString { get; private set;}
        public string TransmitString { get; set; }
        public bool EnableFeedback { set; private get; }

        public bool RTS
        {
            get { return _port.RTS; }
            set { _port.RTS = value; }
        }

        public bool CTS
        {
            get { return _port.CTS; }
        }

        public bool CD
        {
            get
            {
                if (_port is ComPort)
                {
                    return (_port as ComPort).CD;
                }
                return false;
            }
        }

        public bool DSR
        {
            get
            {
                if (_port is ComPort)
                {
                    return (_port as ComPort).DSR;
                }
                return false;
            }
        }
        public bool RING
        {
            get
            {
                if (_port is ComPort)
                {
                    return (_port as ComPort).RING;
                }
                return false;
            }
        }
        public bool SupportsRTSCTS
        {
            get { return _port.SupportsRTSCTS; }
        }
        public bool Supports485
        {
            get { return _port.Supports485; }
        }
        public bool Supports232
        {
            get { return _port.Supports232; }
        }
        public bool Supports422
        {
            get { return _port.Supports422; }
        }

        public bool ReportCTSChanges
        {
            get
            {
                if (_port is ComPort)
                {
                    return (_port as ComPort).ReportCTSChanges;
                }
                return false;
            }
        }

        public eComBaudRates BaudRate
        {
            get { return (eComBaudRates)_port.BaudRate; }

        }

        public eComDataBits DataBits
        {
            get { return (eComDataBits)_port.DataBits; }
        }

        public eComParityType Parity
        {
            get { return (eComParityType)_port.Parity; }
        }
        public eComStopBits StopBits
        {
            get { return (eComStopBits)_port.StopBits; }

        }
        public eComProtocolType Protocol
        {
            get { return (eComProtocolType)_port.Protocol; }
        }
        public eComHardwareHandshakeType HwHandShake
        {
            get { return (eComHardwareHandshakeType)_port.HwHandShake; }
        }
        public eComSoftwareHandshakeType SwHandShake
        {
            get { return (eComSoftwareHandshakeType)_port.SwHandShake; }
        }

        public uint ComPort_Capabilities
        {
            get
            {
                if (_port is ComPort)
                {
                    return (_port as ComPort).ComPort_Capabilities;
                }
                return (uint)ComPort.eComportCapabilities.COMPORT_SUPPORTS_RS232;
            }
        }
        public uint SupportedBaudRates
        {
            get { return _port.SupportedBaudRates; }
        }

        private CCriticalSection _serialDataReceivedLock = new CCriticalSection();
        private event Action<IComPort, string, eStringEncoding> _serialDataReceived;
        public event Action<IComPort, string, eStringEncoding> SerialDataReceived
        {
            add
            {
                try
                {
                    _serialDataReceivedLock.Enter();
                    _serialDataReceived += value;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Crestron.RAD.ProTransports add _serialDataReceived - {0}", e.ToString());
                }
                finally
                {
                    _serialDataReceivedLock.Leave();
                }
            }

            remove
            {
                try
                {
                    _serialDataReceivedLock.Enter();
                    _serialDataReceived -= value;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Crestron.RAD.ProTransports remove _serialDataReceived - {0}", e.ToString());
                }
                finally
                {
                    _serialDataReceivedLock.Leave();
                }
            }
        }

    }
}
