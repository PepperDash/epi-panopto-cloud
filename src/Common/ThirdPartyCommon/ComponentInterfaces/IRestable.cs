using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IRestable
    {
        void Post(string message, object[] parameters, Action<string> callback);
        void Put(string message, object[] parameters, Action<string> callback);
        void Get(string message, object[] parameters, Action<string> callback);
        void Delete(string message, object[] parameters, Action<string> callback);
        void Patch(string message, object[] parameters, Action<string> callback);
    }
}