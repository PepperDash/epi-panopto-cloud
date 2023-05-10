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
using Crestron.SimplSharp.Net.Http;

namespace Crestron.Panopto.Common.Transports
{
    public class HttpTransport : ATransportDriver
    {
        private string _ipAddress;
        private int _port;
        private HttpClient _client;

        public bool Connected { get; private set; }

        public HttpTransport()
        {
            IsEthernetTransport = true;
            _client = new HttpClient();
        }

        public void Initialize(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress.ToString();
            _port = port;
        }

        public override void Start()
        {
            if (_client == null)
            {
                _client = new HttpClient();
            }
        }

        public override void Stop()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }

        public override void SendMethod(string message, object[] paramaters)
        {
            HttpClientRequest request = new HttpClientRequest();
            RequestType requestType = RequestType.Post;

            if (paramaters != null)
            {
                foreach (object obj in paramaters)
                {
                    if (obj.GetType() == typeof(RequestType))
                    {
                        requestType = (RequestType)obj;
                        break;
                    }
                }
            }

            request.Url.Parse(String.Format("http://{0}:{1}/{2}", _ipAddress, _port, message));
            request.RequestType = requestType;
            request.KeepAlive = false;

            var responseCode = _client.DispatchAsync(request, HostResponseCallback);
            if (responseCode != HttpClient.DISPATCHASYNC_ERROR.PENDING &&
                responseCode != HttpClient.DISPATCHASYNC_ERROR.PROCESS_BUSY)
            {
                HandleConnectionStatus(false);
            }
        }

        public void HostResponseCallback(HttpClientResponse response, HTTP_CALLBACK_ERROR Error)
        {
            try
            {
                switch (Error)
                {
                    case HTTP_CALLBACK_ERROR.COMPLETED:
                        HandleConnectionStatus(true);
                        break;
                    case HTTP_CALLBACK_ERROR.INVALID_PARAM:
                    case HTTP_CALLBACK_ERROR.UNKNOWN_ERROR:
                        HandleConnectionStatus(false);
                        break;
                }


                if (response != null &&
                    DataHandler != null)
                {
                    try
                    {
                        DataHandler(response.Header + response.ContentString);
                    }
                    catch (Exception e)
                    {
                        if (EnableLogging)
                        {
                            Log(string.Format("Exception in HostResponseCallback: {0}", e.Message));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Log(string.Format("Exception while handling HostResponseCallback: {0}", e.Message));
                }
            }
        }

        private void HandleConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                if (!Connected)
                {
                    Connected = true;
                    if (ConnectionChanged != null)
                    {
                        ConnectionChanged(Connected);
                    }
                }
            }
            else
            {
                if (Connected)
                {
                    Connected = false;
                    if (ConnectionChanged != null)
                    {
                        ConnectionChanged(Connected);
                    }
                }
            }
        }
    }
}
