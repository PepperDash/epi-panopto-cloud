using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IDeviceVersioning
    {
        bool SupportsRequestSoftwareVersion { get; }
        void RequestSoftwareVersion();

        //Some devices support a method of checking to see if the older version of software a program
        //was designed against will function with the newer firmware. The response tends to be a list
        //of functions that will work. This can be used if supported to modify the supports bools
        //at run time in the event that the device has a more recent software version and enables us
        //to capture not support logs cleanly instead of failing to simply fail to function.
        bool SupportsRequestSoftwareVersionDifferences { get; }
        void RequestSoftwareVersionDifferences();
    }
}