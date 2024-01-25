using System;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.PanoptoCloud.EpiphanPearl.Interfaces;
using PepperDash.Essentials.PanoptoCloud.EpiphanPearl.Utilities;

namespace PepperDash.Essentials.PanoptoCloud.EpiphanPearl
{
    public class EpiphanPearlClient : IEpiphanPearlClient
    {
        private readonly HttpClient _client;

        private readonly HttpHeader _authHeader;

        private readonly string _basePath;

        public EpiphanPearlClient(string host, string username, string password)
        {
            _client = new HttpClient();

            _basePath = string.Format("http://{0}/api", host);

            _authHeader = HttpHelpers.GetAuthorizationHeader(username, password);
        }

        public T Get<T>(string path) where T:class
        {
            var request = CreateRequest(path, RequestType.Get);

            var response = SendRequest(request);

            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception ex)
            {
                Debug.Console(0, "Exception sending to {0}: {1}", request.Url, ex.Message);
                Debug.Console(2, "Stack Trace: {0}", ex.StackTrace);

                if (ex.InnerException == null) return null;

                Debug.Console(0, "Exception sending to {0}: {1}", request.Url, ex.InnerException.Message);
                Debug.Console(2, "Stack Trace: {0}", ex.InnerException.StackTrace);

                return null;
            }
        }

        public TResponse Post<TBody, TResponse> (string path, TBody body) where TBody: class where TResponse: class
        {
            var request = CreateRequest(path, RequestType.Post);

            request.Header.ContentType = "application/json";
            request.ContentString = body != null ? JsonConvert.SerializeObject(body) : string.Empty;

            Debug.Console(2, "Post request: {0} - {1}", request.Url, request.ContentString);

            var response = SendRequest(request);

            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<TResponse>(response);
            }
            catch (Exception ex)
            {
                Debug.Console(0, "Exception sending to {0}: {1}", request.Url, ex.Message);
                Debug.Console(2, "Stack Trace: {0}", ex.StackTrace);

                if (ex.InnerException == null) return null;

                Debug.Console(0, "Exception sending to {0}: {1}", request.Url, ex.InnerException.Message);
                Debug.Console(2, "Stack Trace: {0}", ex.InnerException.StackTrace);

                return null;
            }
        }

        public TResponse Post<TResponse>(string path)
            where TResponse : class
        {
            var request = CreateRequest(path, RequestType.Post);

            request.Header.ContentType = "application/json";

            var response = SendRequest(request);

            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<TResponse>(response);
            }
            catch (Exception ex)
            {
                Debug.Console(0, "Exception sending to {0}: {1}", request.Url, ex.Message);
                Debug.Console(2, "Stack Trace: {0}", ex.StackTrace);

                if (ex.InnerException == null) return null;

                Debug.Console(0, "Exception sending to {0}: {1}", request.Url, ex.InnerException.Message);
                Debug.Console(2, "Stack Trace: {0}", ex.InnerException.StackTrace);

                return null;
            }
        }

        public string Delete(string path)
        {
            var request = CreateRequest(path, RequestType.Delete);

            return SendRequest(request);
        }

        private string SendRequest(HttpClientRequest request)
        {
            try
            {
                var response = _client.Dispatch(request);

                Debug.Console(2, "Response from request to {0}: {1} {2}", request.Url, response.Code,
                    response.ContentString);

                return response.ContentString;
            }
            catch (Exception ex)
            {
                Debug.Console(0, "Exception sending to {0}: {1}", request.Url, ex.Message);
                Debug.Console(2, "Stack Trace: {0}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Debug.Console(0, "Exception sending to {0}: {1}", request.Url, ex.InnerException.Message);
                    Debug.Console(2, "Stack Trace: {0}", ex.InnerException.StackTrace);
                }

                return null;
            }
        }

        private HttpClientRequest CreateRequest(string path, RequestType requestType)
        {
            var request = new HttpClientRequest
            {
                Url = new UrlParser(string.Format("{0}{1}", _basePath, path)),
                RequestType = requestType
            };

            request.Header.AddHeader(_authHeader);

            return request;
        }
    }
}