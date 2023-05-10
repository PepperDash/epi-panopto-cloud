// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Transports
{
    public interface IComPort
    {
        /// <summary>
        /// Function for the user to send a string out the com port.
        /// 
        /// </summary>
        /// <param name="dataToTransmit">Serial data to send out</param><exception cref="T:System.ArgumentNullException">The specified string to transmit is 'null'.</exception>
        void Send(string dataToTransmit);

        /// <summary>
        /// Function to set the COM port's specification as defined in the ComPortSpec structure.
        /// 
        /// </summary>
        /// <param name="paramComPortSpecificationStructure">Structure that contains information about how the serial port will be configured.</param>
        /// <returns>
        /// UShort if sending the new com port specification was successful. 0 = successful.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The COMPORT does not supports a part of the specified configuration. See messages for more information.</exception><exception cref="T:System.InvalidOperationException">The COMPORT is not registered.</exception>
        int SetComPortSpec(ComPortSpec paramComPortSpecificationStructure);

        /// <summary>
        /// Function to unregister the ComPort, if registered.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// Returns device unregistration response
        /// </returns>
        bool UnRegister();

        /// <summary>
        /// Function to get the Rcvdata on the COMPORT
        /// 
        /// </summary>
        string RcvdString { get; }

        /// <summary>
        /// Send data to the COMPORT
        /// 
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The specified string to transmit is 'null'.</exception>
        string TransmitString { get; set; }

        /// <summary>
        /// Enable Feedback for Comports
        ///             Reserved for future use
        /// 
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This function is not supported.</exception>
        bool EnableFeedback { set; }

        /// <summary>
        /// Set the RTS flag.
        /// 
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This COMPORT does not support RTS.</exception>
        bool RTS { get; set; }

        /// <summary>
        /// Get the current CTS flag state
        /// 
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This COMPORT does not support CTS.</exception>
        bool CTS { get; }

        /// <summary>
        /// Get the CD flag
        /// 
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This COMPORT does not support CD.</exception>
        bool CD { get; }

        /// <summary>
        /// Get the DSR state
        /// 
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This COMPORT does not support DSR.</exception>
        bool DSR { get; }

        /// <summary>
        /// Get the RING state
        /// 
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This COMPORT does not support RING.</exception>
        bool RING { get; }

        /// <summary>
        /// See if the COMPORT supports RTS/CTS
        /// 
        /// </summary>
        bool SupportsRTSCTS { get; }

        /// <summary>
        /// See if the COMPORT Supports 485
        /// 
        /// </summary>
        bool Supports485 { get; }

        /// <summary>
        /// See if the COMPORT Supports 232
        /// 
        /// </summary>
        bool Supports232 { get; }

        /// <summary>
        /// See if the COMPORT Supports 422
        /// 
        /// </summary>
        bool Supports422 { get; }

        /// <summary>
        /// Property to return if this COM port will report CTS changes
        /// 
        /// </summary>
        bool ReportCTSChanges { get; }

        /// <summary>
        /// Get the Capabilities of this com port. (RS 232/422/485 and RTS/CTS Support).
        ///             Bitwise against the ComportCapabilitiesEnum.
        /// 
        /// </summary>
        uint ComPort_Capabilities { get; }

        /// <summary>
        /// Get the supported baudrates of this com port.
        ///             Bitwise against the ComBaudRatesEnum.
        /// 
        /// </summary>
        uint SupportedBaudRates { get; }

        /// <summary>
        /// Serial data receive event handler
        /// 
        /// </summary>
        event Action<IComPort, string, eStringEncoding> SerialDataReceived;

    }

    public struct ComPortSpec
    {
        /// <summary>
        /// Indicate to the driver to report CTS state changes.
        /// 
        /// </summary>
        public bool ReportCTSChanges { get; set; }
    }
}
