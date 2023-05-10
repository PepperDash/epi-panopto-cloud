using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.Enums
{
    public enum ErrorState
    {
        Unknown = 0,
        Nominal = 1,
        Generic = 2,
        Missing = 3,
        Unassigned = 4
    }
}