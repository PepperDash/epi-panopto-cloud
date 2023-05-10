using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto
{
    public class RemoteRecorderPreviewData
    {
        public int ThumbnailRefreshIntervalSeconds { get; set; }
        public int RecorderAwakeIntervalSeconds { get; set; }
        public List<Device> Devices { get; set; }
    }
}