using System.Collections.Generic;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.PanoptoCloud
{
    public class PanoptoCloudControllerFactory : EssentialsPluginDeviceFactory<PanoptoCloudController>
    {
        public PanoptoCloudControllerFactory()
        {
            TypeNames = new List<string> {"panopto", "panoptocloud"};
            MinimumEssentialsFrameworkVersion = "1.12.8";
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            return new PanoptoCloudController(dc);
        }
    }
}