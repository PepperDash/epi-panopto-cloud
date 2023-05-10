using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto
{
    public class Device
    {
        public string Name { get; set; }
        public bool IsPrimaryAudio { get; set; }
        public bool IsPrimaryVideo { get; set; }
        public string AudioPreviewUrl { get; set; }
        public string VideoPreviewUrl { get; set; }
    }
}