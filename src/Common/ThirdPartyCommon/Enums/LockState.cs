using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.Enums
{
    /// <summary>
    /// The LockState enum defines the different states of a lock in a device.
    /// </summary>
    public enum LockState
    {
        Unknown = 0,
        Unlocked = 1,
        Locked = 2
    }
}