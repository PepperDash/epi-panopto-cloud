using System.Collections.Generic;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugins
{
    public class PanoptoCloudControllerFactory : EssentialsPluginDeviceFactory<PanoptoCloudController>
    {
        public PanoptoCloudControllerFactory()
        {
            TypeNames = new List<string> {"panopto", "panoptocloud"};
            MinimumEssentialsFrameworkVersion = "3.0.0";
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            return new PanoptoCloudController(dc);
        }
    }
}