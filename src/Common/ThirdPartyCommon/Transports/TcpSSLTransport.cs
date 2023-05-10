using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp.Cryptography.X509Certificates;
using Crestron.Panopto.Common.BasicDriver;

namespace Crestron.Panopto.Common.Transports
{
    public class TcpSSLTransport : ATransportDriver
    {
        private TimelineEventHandler timelineEventTrigger;
        protected SecureTCPClient Client;
        protected int TimeBetweenReconnects = 1000;
        protected string LastMessage;
        private bool _userDisconnect;

        #region Properties

        public bool Connected { set; get; }
        protected bool ReConnecting { set; get; }
        public bool EnableAutoReconnect { get; set; }

        #endregion

        #region Constructors

        public TcpSSLTransport(bool autoReconnect, bool logging, Action<string> customLogger, bool rxDebug, bool txDebug)
        {
            EnableAutoReconnect = autoReconnect;
            EnableLogging = logging;
            CustomLogger = customLogger;
            EnableRxDebug = rxDebug;
            EnableTxDebug = txDebug;
            IsEthernetTransport = true;
        }

        public TcpSSLTransport()
        {
            IsEthernetTransport = true;
        }

        #endregion

        public void Initialize(IPAddress ipAddress, int ipPort)
        {
            Client = new SecureTCPClient
            {
                AddressClientConnectedTo = ipAddress.ToString(),
                PortNumber = ipPort
            };
            Client.SocketStatusChange -= new SecureTCPClientSocketStatusChangeEventHandler(Client_SocketStatusChange);
            Client.SocketStatusChange += new SecureTCPClientSocketStatusChangeEventHandler(Client_SocketStatusChange);
            timelineEventTrigger = new TimelineEventHandler(TimeBetweenReconnects, 0);
            timelineEventTrigger.EventExecute -= Reconnect;
            timelineEventTrigger.EventExecute += Reconnect;
        }

        public void Initialize (IPAddress ipAddress, int ipPort, X509Certificate certificate, byte[] key)
        {
            Client = new SecureTCPClient
            {
                AddressClientConnectedTo = ipAddress.ToString(),
                PortNumber = ipPort,

            };
            Client.SetClientCertificate(certificate);
            Client.SetClientPrivateKey(key);
            Client.SocketStatusChange += new SecureTCPClientSocketStatusChangeEventHandler(Client_SocketStatusChange);
        }

        private void Reconnect()
        {
            if (EnableLogging)
            {
                Log(string.Format("TcpSSLTransport, Attempting to reconnect to IP Address: {0} Port: {1}", Client.AddressClientConnectedTo, Client.PortNumber));
            }

            _userDisconnect = false;

            Client.ConnectToServer();
        }

        void Client_SocketStatusChange(SecureTCPClient myTCPClient, SocketStatus clientSocketStatus)
        {
            Connected = clientSocketStatus == SocketStatus.SOCKET_STATUS_CONNECTED;
            ConnectionChanged(Connected);

            if (Connected == false)
            {
                if (EnableLogging)
                {
                    var loggingStatement = new StringBuilder();
                    loggingStatement.Append("TcpSSLTransport : Disconnected from IP Address: ");
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

                timelineEventTrigger.Start(0);
            }
            else
            {
                if (EnableLogging)
                {
                    var loggingStatement = new StringBuilder();
                    loggingStatement.Append("TcpSSLTransport, Connection to IP Address: ");
                    loggingStatement.Append(Client.AddressClientConnectedTo);
                    loggingStatement.Append(" Port: ");
                    loggingStatement.Append(Client.PortNumber);
                    loggingStatement.Append(Connected ? " is connected." : " could not connect.");

                    Log(loggingStatement.ToString());
                }

                Client.ReceiveDataAsync(ReceiveData);
                TimeBetweenReconnects = 1000;
            }
        }

        public void ReceiveData(SecureTCPClient client, int size)
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

        private void ClientReconnectToServerCallback(object obj)
        {
            if (Client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                if (EnableLogging)
                {
                    var loggingStatement = new StringBuilder();
                    loggingStatement.Append("TcpSSLTransport, Connection to server at ");
                    loggingStatement.Append(Client.AddressClientConnectedTo);
                    loggingStatement.Append(":");
                    loggingStatement.Append(Client.PortNumber);
                    loggingStatement.Append(" failed. Reason=");
                    loggingStatement.Append(Client.ClientStatus.ToString());
                    Log(loggingStatement.ToString()); 
                }
            }

            Connected = Client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED;


            if (Connected)
            {
                timelineEventTrigger.Stop();
            }
        }

        public override void SendMethod(string message, object[] paramaters)
        {
            if (!Connected)
            {
                return;
            }

            var buf = Encoding.GetBytes(message);
            LastMessage = message;

            Client.SendDataAsync(buf, buf.Length, SendDataCallback);
        }

        private void SendDataCallback(SecureTCPClient Client, int numberOfBytesSent)
        {
        }

        public override void Start()
        {
            try
            {
                Log(string.Format("TcpSSLTransport, Attempting to connect to IP Address: {0} Port: {1}",
                    Client.AddressClientConnectedTo, Client.PortNumber));
                Client.ConnectToServer();
                _userDisconnect = false;
            }
            catch (Exception Ex)
            {
                Log(string.Format("Error TCPSSLTransport err: {0}", Ex.Message));
            }
        }

        public override void Stop()
        {
            timelineEventTrigger.Stop();
            _userDisconnect = true;
            Client.DisconnectFromServer();
        }
    }
}