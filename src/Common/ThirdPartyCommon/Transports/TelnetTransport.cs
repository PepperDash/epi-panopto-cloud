// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace Crestron.Panopto.Common.Transports
{
    public class TelnetTransport : ATransportDriver
    {
        private IPAddress _address;
        private int _port;
        private TCPClient _client;
        protected string LastMessage;
        private StringBuilder _message = new StringBuilder();
        private readonly object _messageLock = new object();
        protected CTimer ReconnectTimer;
        protected CTimer TimeOutTimer;
        private bool _userDisconnect;

        enum Verbs
        {
            Will = 251,
            Wont = 252,
            Do = 253,
            Dont = 254,
            Iac = 255
        }

        enum Options
        {
            Sga = 3
        }

        public TelnetTransport()
        {
            IsEthernetTransport = true;
        }

        public void Initialize(IPAddress address, int port)
        {
            _address = address;
            _port = port;
            Send = SendLine;
            IsConnected = false;
            ReconnectTimer = new CTimer(Connect, Timeout.Infinite);
            TimeOutTimer = new CTimer(CommandTimedOut, Timeout.Infinite);
        }

        protected void CommandTimedOut(object nullParam)
        {
            TimeOutTimer.Stop();
            if(MessageTimedOut != null)
                MessageTimedOut(LastMessage);
        }

        private void Connect(object obj)
        {
            var socketErrorCodes = _client.ConnectToServer();
            IsConnected = socketErrorCodes == SocketErrorCodes.SOCKET_OK;
        }

        public void SendLine(string cmd, object[] parameters)
        {
            SendMethod(cmd + "\n", null);
        }

        public override void SendMethod(string message, object[] paramaters)
        {
            if (!IsConnected) return;

            var buf = Encoding.Default.GetBytes(message.Replace("\0xFF", "\0xFF\0xFF"));
            LastMessage = message;
            TimeOutTimer.Reset(TimeOut);
            _client.SendData(buf, buf.Length);
        }

        public void ReceiveData(TCPClient client, int size)
        {
            TimeOutTimer.Stop();

            var rx = new byte[4096];
            Buffer.BlockCopy(_client.IncomingDataBuffer, 0, rx, 0, size);
            var cr = false;

            if (_client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                _client.ReceiveDataAsync(ReceiveData);

            lock (_messageLock)
            {
                for (var i = 0; i < size; i++)
                {
                    switch (rx[i])
                    {
                        case (int)Verbs.Iac:
                            cr = false;
                            // interpret as command
                            int inputverb = rx[++i];
                            if (inputverb == -1) break;
                            switch (inputverb)
                            {
                                case (int)Verbs.Iac:
                                    //literal IAC = 255 escaped, so append char 255 to string
                                    _message.Append(inputverb);
                                    break;
                                case (int)Verbs.Do:
                                case (int)Verbs.Dont:
                                case (int)Verbs.Will:
                                case (int)Verbs.Wont:
                                    //CrestronConsole.PrintLine("Wont");
                                    // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                    int inputoption = rx[++i];
                                    if (inputoption == -1) break;

                                    //_client.SendData(new[] {(byte) Verbs.Iac}, 1);
                                    SendByte(new[] { (byte)Verbs.Iac });


                                    //_client.SendData(
                                    SendByte(
                                        inputoption == (int)Options.Sga
                                            ? new[] { inputverb == (int)Verbs.Do ? (byte)Verbs.Will : (byte)Verbs.Do }
                                            : new[] { inputverb == (int)Verbs.Do ? (byte)Verbs.Wont : (byte)Verbs.Dont });
                                    //_client.SendData(new[] {(byte) inputoption}, 1);
                                    SendByte(new[] { (byte)inputoption });
                                    break;
                            }
                            break;
                        case 0x0d:
                            cr = true;
                            break;
                        case 0x0a:
                            if (cr)
                            {
                                if (!string.IsNullOrEmpty(_message.ToString()) && DataHandler != null)
                                {
                                    DataHandler(_message.ToString());
                                }
                                _message = new StringBuilder();
                            }
                            break;
                        default:
                            _message.Append((char)rx[i]);
                            break;
                    }
                }

                if (cr) return;

                if (!string.IsNullOrEmpty(_message.ToString()) && DataHandler != null)
                {
                    DataHandler(_message.ToString());
                }
                _message = new StringBuilder();
            }
        }

        private void SendByte(byte[] b)
        {
            //CrestronConsole.Print("Send Byte: ");
            //CrestronConsole.PrintLine(b.ToString());
            _client.SendData(b,1);
        }

        public override void Start()
        {
            //CrestronConsole.PrintLine("TelnetTransport Start Initialize Client.");
            _client = new TCPClient { AddressClientConnectedTo = _address.ToString(), PortNumber = _port };
            _client.SocketStatusChange += TCP_SocketStatusChange;
            //CrestronConsole.PrintLine("TelnetTransport ConnectToServer.");
            _client.ConnectToServer();
        }

        void TCP_SocketStatusChange(TCPClient myTcpClient, SocketStatus clientSocketStatus)
        {
            IsConnected = clientSocketStatus == SocketStatus.SOCKET_STATUS_CONNECTED;

            if(_userDisconnect) return;

            if (IsConnected == false)
            {
                ReconnectTimer.Reset(1000, 15000); //start in 1 second repeate every 15
            }
            else
            {
                ReconnectTimer.Stop();
                //CrestronConsole.PrintLine("TelnetTransport call ReceiveDataAsync.");
                _client.ReceiveDataAsync(ReceiveData);
            }

            if(ConnectionChanged != null)
                ConnectionChanged(IsConnected);
        }

        public override void Stop()
        {
            //CrestronConsole.PrintLine("TelnetTransport DisconnectFromServer.");
            _userDisconnect = true;
            _client.DisconnectFromServer();
        }
    }
}
