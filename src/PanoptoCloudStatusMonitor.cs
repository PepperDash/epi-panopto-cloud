﻿using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PanoptoCloudEpi
{
    public class PanoptoCloudStatusMonitor : StatusMonitorBase
    {
        private bool _isStarted;

        public PanoptoCloudStatusMonitor(IKeyed parent, long warningTime, long errorTime) : base(parent, warningTime, errorTime)
        {
        }

        public override void Start()
        {
            _isStarted = true;
            UpdateTimers();
        }

        public override void Stop()
        {
            _isStarted = false;
            StopErrorTimers();
        }

        public void SetOnlineStatue(bool isOnline)
        {
            if (isOnline)
                Status = MonitorStatus.IsOk;

            UpdateTimers();
        }

        public void UpdateTimers()
        {
            if (!_isStarted)
                return;

            if (Status == MonitorStatus.IsOk)
            {
                StopErrorTimers();
            }
            else
            {
                StartErrorTimers();
            }
        }
    }
}