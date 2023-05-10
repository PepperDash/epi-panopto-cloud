using Crestron.RAD.Common;
using Crestron.RAD.Common.Enums;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crestron.RAD.BaseDriver
{
    public class BaseRootObject
    {
        [JsonPropertyAttribute("BaseDeviceCrestronSerialDeviceApi")]
        public CrestronSerialDeviceApi CrestronSerialDeviceApi { get; set; }
    }

    public class CrestronSerialDeviceApi
    {
        public GeneralInformation GeneralInformation { get; set; }

        [JsonPropertyAttribute("BaseDeviceApi")]
        public Api Api { get; set; }

        [JsonPropertyAttribute("BaseDeviceDeviceSupport")]
        public Dictionary<CommonFeatureSupport, bool> DeviceSupport;
    }
    
    public class Api
    {
        public Communication Communication { get; set; }
        public Dictionary<StandardCommandsEnum, Commands> StandardCommands { get; set; }
        public List<CustomCommand> CustomCommands { get; set; }

        [JsonPropertyAttribute("BaseDeviceFeedback")]
        public Feedback Feedback { get; set; }
    }

    public class Feedback
    {
        public CommandAckNak CommandAckNak { get; set; }
        public string Header { get; set; }
    }

}