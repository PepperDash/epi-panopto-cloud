// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.Text;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Transports;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;

namespace Crestron.RAD.ProTransports
{
    public class CommonSerialComport : ATransportDriver
    {
        private readonly Queue<string> _receiveQueue;
        private readonly IComPort _comport;
        private bool _processing;
        private CCriticalSection _queueLock;

        public CommonSerialComport(IComPort comport)
        {
            _queueLock = new CCriticalSection();
            _receiveQueue = new Queue<string>(200);
            _comport = comport;
        }

        public override void Start()
        {
            _processing = true;
            CrestronInvoke.BeginInvoke(Process);
            _comport.SerialDataReceived -= DataReceived;
            _comport.SerialDataReceived += DataReceived;
        }

        public override void Stop()
        {
            _processing = false;
            if (ConnectionChanged != null)
            {
                ConnectionChanged(false);
            }
        }

        public override void SendMethod(string message, object[] parameters)
        {
            try
            {
                if (!_processing)
                {
                    if (ConnectionChanged != null)
                    {
                        ConnectionChanged(false);
                    }
                    return;
                }
                _comport.Send(message);
            }
            catch (Exception e)
            {
                ErrorLog.Notice("Exception occured while transmitting data to the COM port. Exception: {0}", e.ToString());
            }
        }

        private void DataReceived(IComPort comport, string rx, eStringEncoding encoding)
        {
            if (!string.IsNullOrEmpty(rx))
            {
                try
                {
                    _queueLock.Enter();
                    _receiveQueue.Enqueue(rx);
                }
                catch (Exception e)
                {
                    ErrorLog.Notice("Exception occured while enqueuing received data for driver ID {0} - Exception: {1}", DriverID, e.Message);
                }
                finally
                {
                    _queueLock.Leave();
                }
                if (EnableRxDebug)
                {
                    var rxBytes = Encoding.GetBytes(rx);
                    var debugStringBuilder = new StringBuilder("RX: ");
                    debugStringBuilder.Append(LogTxAndRxAsBytes ? BitConverter.ToString(rxBytes).Replace("-", " ") : rx);
                    debugStringBuilder.Append('\n');
                    Log(debugStringBuilder.ToString());
                }
            }
        }

        private void Process(object obj)
        {
            while (_processing)
            {
                var packet = string.Empty;
                try
                {
                    _queueLock.Enter();
                    if (_receiveQueue.Count != 0)
                    {
                        packet = _receiveQueue.Dequeue();
                    }
                }
                catch (Exception e)
                {
                    ErrorLog.Notice("Exception occured while dequeueing received data for driver ID {0} - Exception: {1}", DriverID, e.Message);
                }
                finally
                {
                    _queueLock.Leave();
                }

                if (!string.IsNullOrEmpty(packet) &&
                    DataHandler != null)
                {
                    for (int i = 0; i < packet.Length; i++)
                    {
                        DataHandler(Convert.ToString(packet[i]));
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        [System.Obsolete("This is a deprecated method.", false)]
        protected virtual void ResponseTimerExpired(object nullParam)
        { }
    }
}
