using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Https;

namespace Crestron.Panopto
{
    public class HttpsResponseArgs
    {
        public string Url { get; set; }
        public string ResponseString { get; set; }
        public byte[] ResponseBytes { get; set; }
        public int ResponseCode { get; set; }
        public HttpsHeaders Headers { get; set; }
        public Command Command { get; set; }
    }
}