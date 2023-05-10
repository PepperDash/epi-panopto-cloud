using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.RAD.Common.Enums;

namespace Crestron.RAD.BaseDriver
{
    public class ValidatedRxData
    {
        public ValidatedRxData(bool ready, string data)
        {
            Ready = ready;
            Data = data;
            Ignore = false;
            CommandGroup = CommonCommandGroupType.Unknown;
        }

        public bool Ready { get; set; }
        public bool Ignore { get; set; }
        public string Data { get; set; }
        public string CustomCommandGroup { get; set; }
        public CommonCommandGroupType CommandGroup { get; set; }
    }
}