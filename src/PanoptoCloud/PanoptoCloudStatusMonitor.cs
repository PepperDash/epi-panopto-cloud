using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugins
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

            if (Status == MonitorStatus.IsOk)
                StopErrorTimers();
            else
                StartErrorTimers();
        }

        public override void Stop()
        {
            _isStarted = false;
            StopErrorTimers();
        }

        /// <summary>
        /// Report the outcome of a request.
        /// </summary>
        /// <remarks>
        /// A failure starts the warning and error timers rather than changing the status itself, so
        /// one slow reply does not take the recorder offline but a server that stays silent does.
        /// <see cref="StatusMonitorBase.IsOnline"/> follows the status — it used to be written
        /// straight from each reply, while the status itself stuck at OK after the first success.
        /// </remarks>
        public void SetOnlineStatus(bool isOnline)
        {
            if (isOnline)
            {
                Status = MonitorStatus.IsOk;
                StopErrorTimers();
                return;
            }

            // StartErrorTimers leaves timers that are already running alone, so repeated failures
            // count from the first one.
            if (_isStarted)
                StartErrorTimers();
        }
    }
}
