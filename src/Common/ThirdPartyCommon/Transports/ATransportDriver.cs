// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Text;
using Crestron.Panopto.Common.Enums;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.Transports
{
    public abstract class ATransportDriver : ISerialTransport
    {
        public Action<string> DataHandler { get; set; }
        public Action<string> MessageTimedOut { get; set; }
        public Action<bool> ConnectionChanged { get; set; }
        public Encoding Encoding { get; private set; }
        public int DriverID { get; set; }
        public bool IsConnected { get; set; }
        public uint TimeOut { get; set; }

        public abstract void SendMethod(string message, object[] paramaters);
        public abstract void Start();
        public abstract void Stop();

        protected Action<string, object[]> InternalSend { get; set; }
        public Action<string, object[]> Send
        {
            get { return TxDebugThenSend; }
            set { InternalSend = value; }
        }

        protected void TxDebugThenSend(string message, object[] paramaters)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    if (paramaters != null && EnableLogging)
                    {
                        Log("Crestron.Panopto.Common.Transports.ATransportDriver.TxDebugThenSend Warning: parameters is null");
                    }
                    if (EnableTxDebug)
                    {
                        var buf = Encoding.GetBytes(message);
                        var debugStringBuilder = new StringBuilder("TX: ");
                        debugStringBuilder.Append(LogTxAndRxAsBytes ? BitConverter.ToString(buf).Replace("-", " ") : message);
                        debugStringBuilder.Append('\n');
                        Log(debugStringBuilder.ToString());
                    }
                    InternalSend(message, paramaters);
                }
                else if (EnableLogging)
                {
                    Log("Crestron.Panopto.Common.Transports.ATransportDriver.TxDebugThenSend Notice: message is empty or null");
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Crestron.Panopto.Common.Transports.ATransportDriver.TxDebugThenSend error message: {0}", ex.Message));
            }
        }

        public bool LogTxAndRxAsBytes { get; set; }
        public bool EnableTxDebug { get; set; }
        public bool EnableRxDebug { get; set; }
        public bool EnableLogging { get; set; }
        public bool IsEthernetTransport { get; set; }
        public Action<string> CustomLogger { get; set; }

        protected virtual void Log(string message)
        {
            if (!EnableLogging) return;

            message = string.Format("{0}::{1}::({2}) {3}",
                        CrestronEnvironment.TickCount,
                        DateTime.Now,
                        DriverID,
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

        protected ATransportDriver()
        {
            LogTxAndRxAsBytes = true;
            Send = SendMethod;
            DriverID = 0;
            Encoding = Encoding.GetEncoding("ISO-8859-1");
            IsEthernetTransport = false;
        }

        public TransportType TransportType { get { return TransportType.Serial; } }
    }
}
