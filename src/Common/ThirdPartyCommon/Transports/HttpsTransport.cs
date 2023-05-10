// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp.Ssh;
using RequestType = Crestron.SimplSharp.Net.Https.RequestType;

namespace Crestron.Panopto.Common.Transports
{
    public class HttpsTransport : ATransportDriver
    {
        public bool HttpsClientHostVerification { get; set; }
        public bool HttpsClientPeerVerification { get; set; }
        public bool HttpsClientKeepAlive { get; set; }
        public bool HttpsClientVerbose { get; set; }
        public bool HttpsClientIncludeHeaders { get; set; }
        public bool HttpsClientSecure { get; set; }
        public string HttpsUserName;
        public string HttpsPassword;


        private string _ipAddress;
        private int _port;

        public bool Connected { get; private set; }

        public HttpsTransport()
        {
            IsEthernetTransport = true;
        }

        public void Initialize(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress.ToString();
            _port = port;
        }

        public override void Start()
        { }

        public override void Stop()
        { }

        private HttpsClientRequest BuildRequest(string message, object[] paramaters)
        {
            HttpsClientRequest req = new HttpsClientRequest();
            if (!String.IsNullOrEmpty(message))
            {
                req.ContentString = message;
            }
            RequestType requestType = RequestType.Post;//default request type, overriden by parameters

            foreach (object obj in paramaters)
            {
                if (obj.GetType() == typeof(RequestType))
                {
                    requestType = (RequestType)obj;
                }
                else if (obj.GetType() == typeof(HttpsHeader))
                {
                    req.Header.AddHeader(obj as HttpsHeader);
                }
            }
            var uri = new Uri(String.Format("https://{0}:{1}", _ipAddress, _port));
            
            req.RequestType = requestType;
            req.Url.Parse(uri.ToString()); 

            return req;
        }

        public override void SendMethod(string message, object[] paramaters)
        {
            using (HttpsClient client = new HttpsClient())
            {
                var request = BuildRequest(message, paramaters);
                try
                {
                    client.HostVerification = HttpsClientHostVerification;
                    client.PeerVerification = HttpsClientPeerVerification;
                    client.IncludeHeaders = HttpsClientIncludeHeaders;
                    client.KeepAlive = HttpsClientKeepAlive;
                    client.Verbose = HttpsClientVerbose;

                    client.UserName = HttpsUserName ?? string.Empty;
                    client.Password = HttpsPassword ?? string.Empty;
                   
                    var response = client.Dispatch(request);
                    if (response != null)
                    {
                        if (!Connected)
                        {
                            Connected = true;
                            if (ConnectionChanged != null)
                            {
                                ConnectionChanged(Connected);
                            }
                        }

                        if (DataHandler != null)
                        {
                            DataHandler(response.Header + response.ContentString);
                        }
                    }
                }
                catch (HttpsException httpsException)
                {
                    if (EnableLogging)
                    {
                        Log(string.Format("HTTPS Method Exception: {0}", httpsException.Message));
                    }
                    if (!Connected)
                    {
                        Connected = true;
                        if (ConnectionChanged != null)
                        {
                            ConnectionChanged(Connected);
                        }
                    }

                    if (DataHandler != null)
                    {
                        DataHandler(httpsException.Response.Header + httpsException.Response.ContentString);
                    }
                }
                catch (Exception e)
                {
                    if (EnableLogging)
                    {
                        Log(string.Format("Send Method Exception: {0}", e.Message));
                    }
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
