using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Crestron.Panopto.Common.Interfaces;
using Crestron.Panopto.Common.Transports;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Https;

namespace Crestron.Panopto.Common.Transports
{

    public class RestTransport : ATransportDriver, IRestable
    {
        private Uri _host;
        private HttpsClient _client;

        public RestTransport()
        {
            IsEthernetTransport = true;
        }

        public void Initialize(Uri hostName, HttpsClient client)
        {
            _host = hostName;
            _client = client;
        }

        public void Post(string message, object[] parameters, Action<string> callback)
        {
            DataHandler = callback;
            List<object> paramList = parameters.ToList();
            paramList.Add(RequestType.Post);
            SendMethod(message, paramList.ToArray());
        }

        public void Put(string message, object[] parameters, Action<string> callback)
        {
            DataHandler = callback;
            List<object> paramList = parameters.ToList();
            paramList.Add(RequestType.Put);
            SendMethod(message, paramList.ToArray());
        }

        public void Delete(string message, object[] parameters, Action<string> callback)
        {
            DataHandler = callback;
            List<object> paramList = parameters.ToList();
            paramList.Add(RequestType.Delete);
            SendMethod(message, paramList.ToArray());
        }

        public void Patch(string message, object[] parameters, Action<string> callback)
        {
            DataHandler = callback;
            List<object> paramList = parameters.ToList();
            paramList.Add(RequestType.Patch);
            SendMethod(message, paramList.ToArray());
        }

        public void Get(string message, object[] parameters, Action<string> callback)
        {
            DataHandler = callback;
            List<object> paramList = parameters.ToList();
            paramList.Add(RequestType.Get);
            SendMethod(message, paramList.ToArray());
        }

        public override void SendMethod(string message, object[] parameters)
        {
            var req = BuildRequest(message, parameters);
            _client.Url = req.Url;
            string headers = string.Empty;
            foreach (HttpsHeader header in req.Header)
            {
                //CrestronConsole.PrintLine("Rest Transport: {0}", header);
                headers = String.Format("{0}\n", header);
            }
            //CrestronConsole.PrintLine("Sending URL: {0} \n Sending message: {1} \n Sending headers: {2}", req.Url, req.ContentString, headers);
            TrySendRequest(req);
        }

        private void TrySendRequest(HttpsClientRequest req)
        {
            try
            {
                //CrestronConsole.PrintLine("Trying to {0} {1}", req.RequestType ,req.Url);
                
                var response = _client.Dispatch(req);
                /*CrestronConsole.PrintLine("response content length: {0}", response.ContentLength);
                CrestronConsole.PrintLine("response code: {0}",response.Code);
                foreach (HttpsHeader header in response.Header)
                {
                    CrestronConsole.PrintLine("response header: {0}", header);
                }
                CrestronConsole.PrintLine("response content string: {0}", response.ContentString);
                CrestronConsole.PrintLine("response content stream: {0}", response.ContentStream);
                CrestronConsole.PrintLine("response content bytes: {0}", response.ContentBytes);
                */if (response != null && DataHandler != null)
                {
                    DataHandler.Invoke(response.ContentString);
                }
            }
            catch (HttpsException e)
            {
                DataHandler.Invoke(e.Response.Header + e.Response.ContentString);
                
            }
            catch (Exception e)
            {
                //CrestronConsole.PrintLine("[TEMP][RestTransport] HttpsException in SendMethod");
                //CrestronConsole.PrintLine(e.Message);
                //CrestronConsole.PrintLine(e.StackTrace);
            }
        }

        private HttpsClientRequest BuildRequest(string message, object[] paramaters)
        {
            //CrestronConsole.PrintLine("building request");
            HttpsClientRequest req = new HttpsClientRequest();
            if(!String.IsNullOrEmpty(message))
            {
                req.ContentString = message;
            }
            RequestType requestType = RequestType.Post;//default request type, overriden by parameters
            HttpsHeaders headers = new HttpsHeaders();
            foreach (object obj in paramaters)
            {
                if (obj.GetType() == typeof (RequestType))
                {
                    requestType = (RequestType)obj;
                }
                else if (obj.GetType() == typeof (HttpsHeader))
                {
                    //CrestronConsole.PrintLine("Adding header");
                    req.Header.AddHeader(obj as HttpsHeader);
                }
            }
            req.Url.Parse(String.Format("{0}", _host.ToString()));
            req.RequestType = requestType;
            req.FinalizeHeader();
            return req;
        }

        public override void Start()
        {/*balk*/}

        public override void Stop()
        {/*balk*/}
    }
}