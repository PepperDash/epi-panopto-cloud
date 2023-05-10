using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.Enums
{
    public enum ResourceStatus
    {
        Unknown = 0,
        Unassigned = 1,
        Armed = 2,
        Disarmed = 3,
        Bypassed = 4,
        AlarmTriggered = 5,
        Short = 6,
        Error = 7,
        Missing = 8,
        Normal = 9,
        Off = 10,
        Pulse = 11,
        Steady = 12,
        Temporal = 13,
        Wink = 14,
        PanicAlarm = 15,
        PanicTest = 16,
        Open = 17,
        LowBattery = 18,
        AllOn = 19,
        AllOnInstantArm = 20,
        AwayOn = 21,
        AwayExitDelay = 22,
        AwayEntryDelay = 23,
        PartOnInstant = 24,
        PartOnDelay = 25,
        AllOnEntryDelay = 26,
        AllOnExitDelay = 27,
        PartOnEntryDelay = 28,
        PartOnExitDelay = 29,
        Stay1On = 30,
        Stay2On = 31,
        Trouble = 32
    }
}