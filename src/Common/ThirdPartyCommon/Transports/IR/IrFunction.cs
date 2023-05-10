using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Transports
{
    internal class IrFunction
    {
        public byte Id;
        public string Label;
        public StandardCommandsEnum FrameworkStandardCommand;
        public byte[] Payload;
    }
}