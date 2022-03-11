using System.Collections.Generic;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PanoptoCloudEpi
{
    public class PanoptoCloudControllerFactory : EssentialsPluginDeviceFactory<PanoptoCloudController>
    {
        public PanoptoCloudControllerFactory()
        {
            TypeNames = new List<string> {"panopto", "panoptocloud"};
            MinimumEssentialsFrameworkVersion = "1.9.7";
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            return new PanoptoCloudController(dc);
        }
    }
}