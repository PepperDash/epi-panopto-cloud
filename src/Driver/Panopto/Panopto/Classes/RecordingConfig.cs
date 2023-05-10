using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;

namespace Crestron.Panopto
{
    public class RecordingConfig
    {
        public string RecordingName { get; set; }
        public bool IsBroadcast { get; set; }
        public DateTime StartTime { get; set;}
        public DateTime EndTime { get; set; }
        public DateTime PauseTime { get; set; }
        public double Duration { get; set;}
        public Guid RecordingId { get; set; }
        public Guid PublicSessionId { get; set; }
        public Guid PauseId { get; set; }
        public Guid FolderId { get; set; }

        public void ImportPanoptoSession(PanoptoSession sessionInfo)
        {
            RecordingName = sessionInfo.Name;
            IsBroadcast = sessionInfo.IsBroadcast;
            Duration = sessionInfo.Duration;
            StartTime = sessionInfo.StartTime;
            EndTime = StartTime.AddSeconds(Duration);
            RecordingId = sessionInfo.RecordingId;
        }

        public override string ToString()
        {
            return string.Format("RecordingName is {0} StartTime is {1} Duration is {2}", RecordingName, StartTime, Duration);
        }
    }
}