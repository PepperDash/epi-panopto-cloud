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
using System.Collections;

namespace Crestron.Panopto.Common.Transports
{
    public class TcpTransport : ATransportDriver
    {
        protected TCPClient Client;
        protected CTimer ReconnectTimer;
        protected int TimeBetweenReconnects = 1000; //Needs to match value of _defaultTimeBetweenReconnects
        protected string LastMessage;
        private bool _userDisconnect;

        private int _defaultTimeBetweenReconnects = 1000;
        private IPAddress _ipAddress;
        private int _port;

        private bool _waitingForAsyncSendCallback;
        private int _firstAsyncCallbackFailureTick = 0;
        private const int _maxTicksToWaitToAllowAsyncSend = 1000;
        private CCriticalSection _waitingForAsyncSendLock;

        /// <summary>
        /// True if a SendDataAsync was called on <see cref="Client"/> and <see cref="SendDataCallback"/> 
        /// hasn't been invoked by the client. This is used to prevent multiple SendDataAsync calls when the previous 
        /// one has not finished.
        /// </summary>
        internal bool WaitingForAsyncSendCallback
        {
            get
            {
                bool enteredLock = false;
                try
                {
                    enteredLock = _waitingForAsyncSendLock.TryEnter();

                    if (enteredLock)
                    {
                        CheckForSendCallbackTimeout();
                        return _waitingForAsyncSendCallback;
                    }
                }
                catch (Exception e)
                {
                    if (EnableLogging)
                    {
                        Log(string.Format("TcpTransport : exception in SendMethod: {0}",
                            e));
                    }
                }
                finally
                {
                    if (enteredLock)
                    {
                        _waitingForAsyncSendLock.Leave();
                    }
                }

                // By default we will wait since this will try to enter the lock, while
                // the callback will enter it, so if it is still in the callback, it is not done
                // sending.
                return true;
            }
        }


        #region Properties

        public bool Connected { set; get; }
        protected bool ReConnecting { set; get; }
        public bool EnableAutoReconnect { get; set; }

        #endregion

        #region Constructors

        public TcpTransport(bool autoReconnect, bool logging, Action<string> customLogger, bool rxDebug, bool txDebug)
        {
            EnableAutoReconnect = autoReconnect;
            EnableLogging = logging;
            CustomLogger = customLogger;
            EnableRxDebug = rxDebug;
            EnableTxDebug = txDebug;
            IsEthernetTransport = true;
            _waitingForAsyncSendLock = new CCriticalSection();
        }

        public TcpTransport()
        {
            IsEthernetTransport = true;
            _waitingForAsyncSendLock = new CCriticalSection();
        }

        #endregion

        public void Initialize(IPAddress ipAddress, int ipPort)
        {
            _ipAddress = ipAddress;
            _port = ipPort;
            CreateClient();
        }

        private void Reconnect(object obj)
        {
            CreateClient();
            if (EnableLogging)
            {
                var loggingStatement = new StringBuilder();
                loggingStatement.Append("TcpTransport : Atempting to reconnect to IP Address: ");
                loggingStatement.Append(Client.AddressClientConnectedTo);
                loggingStatement.Append(" Port: ");
                loggingStatement.Append(Client.PortNumber);

                Log(loggingStatement.ToString());
            }

            _userDisconnect = false;

            if (Client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                Client.ConnectToServerAsync(ClientReconnectToServerCallback);
            }
        }

        private void TCP_SocketStatusChange(TCPClient myTcpClient, SocketStatus clientSocketStatus)
        {
            Connected = clientSocketStatus == SocketStatus.SOCKET_STATUS_CONNECTED;
            if (ConnectionChanged != null)
            {
                ConnectionChanged(Connected);
            }
            if (Connected == false)
            {
                if (EnableLogging)
                {
                    var loggingStatement = new StringBuilder();
                    loggingStatement.Append("TcpTransport : Disconnected from IP Address: ");
                    loggingStatement.Append(Client.AddressClientConnectedTo);
                    loggingStatement.Append(" Port: ");
                    loggingStatement.Append(Client.PortNumber);
                    loggingStatement.Append(" Reason: ");
                    loggingStatement.Append(clientSocketStatus.ToString());

                    Log(loggingStatement.ToString());
                }

                if (_userDisconnect || !EnableAutoReconnect)
                {
                    return;
                }

                if (ReconnectTimer != null && !ReconnectTimer.Disposed)
                {
                    ReconnectTimer.Reset(_defaultTimeBetweenReconnects);
                }
            }
            else
            {
                if (EnableLogging)
                {
                    var loggingStatement = new StringBuilder();
                    loggingStatement.Append("TcpTransport : Connection to IP Address: ");
                    loggingStatement.Append(Client.AddressClientConnectedTo);
                    loggingStatement.Append(" Port: ");
                    loggingStatement.Append(Client.PortNumber);
                    loggingStatement.Append(Connected ? " is connected." : " could not connect.");

                    Log(loggingStatement.ToString());
                }

                if (Client == null)
                {
                    if (EnableLogging)
                    {
                        Log("TcpTransport : Unable to receive data - client does not exist");
                    }
                }
                else
                {
                    Client.ReceiveDataAsync(ReceiveData);
                }
                _defaultTimeBetweenReconnects = TimeBetweenReconnects;
            }
        }

        private void CheckForSendCallbackTimeout()
        {
            if (_firstAsyncCallbackFailureTick == 0 &&
                _waitingForAsyncSendCallback)
            {
                _firstAsyncCallbackFailureTick = CrestronEnvironment.TickCount;
            }
            else if (_firstAsyncCallbackFailureTick != 0 &&
                     Math.Abs(CrestronEnvironment.TickCount - _firstAsyncCallbackFailureTick) > _maxTicksToWaitToAllowAsyncSend)
            {
                _firstAsyncCallbackFailureTick = 0;
                _waitingForAsyncSendCallback = false;
            }
        }

        public override void SendMethod(string message, object[] paramaters)
        {
            if (!Connected)
            {
                if (EnableLogging)
                {
                    Log("TcpTransport : Unable to send data - client is not connected");
                }
                return;
            }

            if (Client != null)
            {
                var buf = Encoding.GetBytes(message);
                LastMessage = message;
                bool enteredLock = false;

                try
                {
                    // By default we will wait since this will try to enter the lock, while
                    // the callback will enter it, so if it is still in the callback, it is not done
                    // sending.
                    enteredLock = _waitingForAsyncSendLock.TryEnter();
                    if (enteredLock)
                    {
                        if (_waitingForAsyncSendCallback)
                        {
                            CheckForSendCallbackTimeout();
                        }
                        if (_waitingForAsyncSendCallback == false)
                        {
                            _waitingForAsyncSendCallback = true;
                            Client.SendDataAsync(buf, buf.Length, SendDataCallback);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (EnableLogging)
                    {
                        Log(string.Format("TcpTransport : exception in SendMethod: {0}",
                            e));
                    }
                }
                finally
                {
                    if (enteredLock)
                    {
                        _waitingForAsyncSendLock.Leave();
                    }
                }
            }
            else if (EnableLogging)
            {
                Log("TcpTransport : Unable to send data - client does not exist");
            }
        }

        private void SendDataCallback(TCPClient myTCPClient, int numberOfBytesSent)
        {
            try
            {
                _waitingForAsyncSendLock.Enter();
                _firstAsyncCallbackFailureTick = 0;
                _waitingForAsyncSendCallback = false;
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Log(string.Format("TcpTransport - exception in SendDataCallback: {0}",
                        e));
                }
            }
            finally
            {
                _waitingForAsyncSendLock.Leave();
            }
        }

        public void ReceiveData(TCPClient client, int size)
        {
            // Upon disconnect this method will be called with an empty packet of size 0; ignore it
            if (size > 0)
            {
                var rx = client.IncomingDataBuffer;
                Buffer.BlockCopy(Client.IncomingDataBuffer, 0, rx, 0, size);
                var message = Encoding.GetString(rx, 0, size);

                if (!string.IsNullOrEmpty(message) &&
                    DataHandler != null)
                {
                    DataHandler(message);
                }

                Client.ReceiveDataAsync(ReceiveData);

                if (EnableRxDebug)
                {
                    var rxBytes = Encoding.GetBytes(message);
                    var debugStringBuilder = new StringBuilder("RX: ");
                    debugStringBuilder.Append(LogTxAndRxAsBytes ? BitConverter.ToString(rxBytes).Replace("-", " ") : message);
                    debugStringBuilder.Append('\n');
                    Log(debugStringBuilder.ToString());
                }
            }
        }

        public override void Start()
        {
            if (_waitingForAsyncSendLock == null ||
                _waitingForAsyncSendLock.Disposed == true)
            {
                _waitingForAsyncSendLock = new CCriticalSection();
            }
            CreateClient();

            if (TimeBetweenReconnects != 1000)
            {
                _defaultTimeBetweenReconnects = TimeBetweenReconnects;
            }

            if (EnableLogging)
            {
                var loggingStatement = new StringBuilder();
                loggingStatement.Append("TcpTransport : Attempting to connect to IP Address: ");
                loggingStatement.Append(Client.AddressClientConnectedTo);
                loggingStatement.Append(" Port: ");
                loggingStatement.Append(Client.PortNumber);
                Log(loggingStatement.ToString());
            }

            _userDisconnect = false;

            if (Client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                Client.ConnectToServerAsync(ClientConnectToServerCallback);
            }
        }

        public override void Stop()
        {
            if (_waitingForAsyncSendLock != null &&
                _waitingForAsyncSendLock.Disposed == true)
            {
                _waitingForAsyncSendLock = new CCriticalSection();
            }

            if (ReconnectTimer != null && !ReconnectTimer.Disposed)
            {
                ReconnectTimer.Stop();
                ReconnectTimer.Dispose();
            }

            _userDisconnect = true;

            SocketErrorCodes returnVal = SocketErrorCodes.SOCKET_OK;
            if (Client != null)
            {
                try
                {
                    returnVal = Client.DisconnectFromServer();
                }
                catch (Exception e)
                {
                    if (EnableLogging)
                    {
                        Log(string.Format("TcpTransport : Unable to disconnect TCPClient from server - {0}", e.Message));
                    }
                }

                // Check again in case the DisconnectFromServer disposes Client
                if (returnVal == SocketErrorCodes.SOCKET_OK ||
                    returnVal == SocketErrorCodes.SOCKET_NOT_CONNECTED)
                {
                    if (EnableLogging)
                    {
                        var loggingStatement = new StringBuilder();
                        loggingStatement.Append("TcpTransport : Disconnected from IP Address: ");
                        loggingStatement.Append(Client.AddressClientConnectedTo);
                        loggingStatement.Append(" Port: ");
                        loggingStatement.Append(Client.PortNumber);
                        loggingStatement.Append(" Reason: ");
                        loggingStatement.Append(Convert.ToString(SocketErrorCodes.SOCKET_NOT_CONNECTED));

                        Log(loggingStatement.ToString());
                    }
                    Client.SocketStatusChange -= TCP_SocketStatusChange;
                    if (ConnectionChanged != null)
                    {
                        ConnectionChanged(false);
                    }
                }
                else
                {
                    if (EnableLogging)
                    {
                        var loggingStatement = new StringBuilder();
                        loggingStatement.Append("TcpTransport : Cound not be Disconnected from IP Address: ");
                        loggingStatement.Append(Client.AddressClientConnectedTo);
                        loggingStatement.Append(" Port: ");
                        loggingStatement.Append(Client.PortNumber);
                        loggingStatement.Append(" Reason: ");
                        loggingStatement.Append(Convert.ToString(returnVal));

                        Log(loggingStatement.ToString());
                    }
                }
            }
            DisposeClient();
        }


        private void ClientConnectToServerCallback(object obj)
        {
            if (Client != null)
            {
                if (Client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
                {
                    if (EnableLogging)
                    {
                        var loggingStatement = new StringBuilder();
                        loggingStatement.Append("TcpTransport, Connection to server at ");
                        loggingStatement.Append(Client.AddressClientConnectedTo);
                        loggingStatement.Append(":");
                        loggingStatement.Append(Client.PortNumber);
                        loggingStatement.Append(" failed. Reason=");
                        loggingStatement.Append(Client.ClientStatus.ToString());
                        Log(loggingStatement.ToString());
                    }

                    if (EnableAutoReconnect)
                    {
                        if (ReconnectTimer != null && !ReconnectTimer.Disposed)
                        {
                            ReconnectTimer.Reset(_defaultTimeBetweenReconnects);
                        }
                    }
                }
                else if (Client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                {
                    Connected = true;
                }
            }
        }

        private void ClientReconnectToServerCallback(object obj)
        {
            if (Client != null)
            {
                if (Client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
                {
                    if (EnableLogging)
                    {
                        var loggingStatement = new StringBuilder();
                        loggingStatement.Append("TcpTransport, Connection to server at ");
                        loggingStatement.Append(Client.AddressClientConnectedTo);
                        loggingStatement.Append(":");
                        loggingStatement.Append(Client.PortNumber);
                        loggingStatement.Append(" failed. Reason=");
                        loggingStatement.Append(Client.ClientStatus.ToString());
                        Log(loggingStatement.ToString());
                    }
                }

                Connected = Client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED;


                if (!Connected)
                {
                    if (_defaultTimeBetweenReconnects < 60000)
                    {
                        _defaultTimeBetweenReconnects += 1000;
                    }

                    if (ReconnectTimer != null && !ReconnectTimer.Disposed)
                    {
                        ReconnectTimer.Reset(_defaultTimeBetweenReconnects);
                    }
                }
                else
                {
                    _defaultTimeBetweenReconnects = TimeBetweenReconnects;

                    if (ReconnectTimer != null && !ReconnectTimer.Disposed)
                    {
                        ReconnectTimer.Stop();
                    }
                }
            }
        }

        private void DisposeClient()
        {
            if (Client == null)
            {
                if (EnableLogging)
                {
                    Log("TcpTransport : Unable to dispose client - client does not exist");
                }
            }
            else
            {
                try
                {
                    Client.Dispose();
                    Client = null;

                    if (EnableLogging)
                    {
                        Log("TcpTransport : Client disposed");
                    }
                }
                catch (Exception e)
                {
                    if (EnableLogging)
                    {
                        Log(string.Format("TcpTransport : Unable to dispose TCPClient - {0}", e.Message));
                    }
                }
            }
        }

        private void CreateClient()
        {
            if (Client == null)
            {
                Client = new TCPClient
                {
                    AddressClientConnectedTo = _ipAddress.ToString(),
                    PortNumber = _port
                };

                Client.SocketStatusChange -= TCP_SocketStatusChange;
                Client.SocketStatusChange += TCP_SocketStatusChange;

                ReconnectTimer = new CTimer(Reconnect, Timeout.Infinite);

                if (EnableLogging)
                {
                    Log(string.Format("TcpTransport : Client created using IP Address: {0} and Port: {1}",
                        _ipAddress.ToString(),
                        _port));
                }
            }
            else
            {
                if (EnableLogging)
                {
                    Log("TcpTransport : Note - reusing existing client");
                }
            }
        }
    }
}
