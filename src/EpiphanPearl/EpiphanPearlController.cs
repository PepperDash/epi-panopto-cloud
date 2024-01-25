using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json.Bson;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.PanoptoCloud.EpiphanPearl.Interfaces;
using PepperDash.Essentials.PanoptoCloud.EpiphanPearl.JoinMaps;
using PepperDash.Essentials.PanoptoCloud.EpiphanPearl.Models;
using PepperDash.Essentials.PanoptoCloud.EpiphanPearl.Utilities;

namespace PepperDash.Essentials.PanoptoCloud.EpiphanPearl
{
    public class EpiphanPearlController : ReconfigurableBridgableDevice, ICommunicationMonitor
    {
        private const string RunningStatus = "running";

        private readonly IEpiphanPearlClient _client;
        private readonly EpiphanCommunicationMonitor _monitor;
        private BoolFeedback _nextEventExistsFeedback;

        private CTimer _pollTimer;

        private Event _runningEvent;

        private StringFeedback _runningEventEndFeedback;
        private StringFeedback _runningEventIdFeedback;
        private StringFeedback _runningEventLengthFeedback;
        private StringFeedback _runningEventNameFeedback;

        private BoolFeedback _runningEventRunningFeedback;
        private StringFeedback _runningEventStartFeedback;
        private FeedbackCollection<StringFeedback> _scheduleEndFeedbacks;
        private FeedbackCollection<StringFeedback> _scheduleIdFeedbacks;
        private FeedbackCollection<StringFeedback> _scheduleLengthFeedbacks;
        private FeedbackCollection<StringFeedback> _scheduleNameFeedbacks;
        private FeedbackCollection<StringFeedback> _scheduleStartFeedbacks;
        private List<Event> _scheduledEvents;
        private CTimer _statusTimer;

        public EpiphanPearlController(DeviceConfig config) : base(config)
        {
            var devConfig = config.Properties.ToObject<EpiphanPearlControllerConfiguration>();

            if (devConfig.Secure)
            {
                _client = new EpiphanPearlSecureClient(devConfig.Host, devConfig.Username, devConfig.Password);
            }
            else
            {
                _client = new EpiphanPearlClient(devConfig.Host, devConfig.Username, devConfig.Password);
            }

            _monitor = new EpiphanCommunicationMonitor(this, 30000, 60000);

            CreateFeedbacks();
        }

        public StatusMonitorBase CommunicationMonitor
        {
            get { return _monitor; }
        }

        public override void Initialize()
        {
            _pollTimer = new CTimer(o => Poll(), null, 0, 60000);

            _monitor.Start();
        }

        private void Poll()
        {
            Debug.Console(0, this, "Getting Scheduled Events");
            GetScheduledEvents();

            Debug.Console(0, this, "Getting Running Events");
            GetRunningEvent();
        }

        private void CreateFeedbacks()
        {
            _scheduleNameFeedbacks = new FeedbackCollection<StringFeedback>();
            _scheduleStartFeedbacks = new FeedbackCollection<StringFeedback>();
            _scheduleEndFeedbacks = new FeedbackCollection<StringFeedback>();
            _scheduleIdFeedbacks = new FeedbackCollection<StringFeedback>();
            _scheduleLengthFeedbacks = new FeedbackCollection<StringFeedback>();

            for (var i = 0; i < 20; i++)
            {
                var index = i;
                var name = new StringFeedback(() => _scheduledEvents.Count > 0 ? _scheduledEvents[index].Title : string.Empty);
                var start = new StringFeedback(() => _scheduledEvents.Count > 0 ? _scheduledEvents[index].Start.ToLocalTime().ToString("hh:mm:ss tt") : string.Empty);
                var end = new StringFeedback(() => _scheduledEvents.Count > 0 ? _scheduledEvents[index].Finish.ToLocalTime().ToString("hh:mm:ss tt"): string.Empty);
                var id = new StringFeedback(() => _scheduledEvents.Count > 0 ? _scheduledEvents[index].Id : string.Empty);
                var length = new StringFeedback(() =>
                {
                    var scheduledEvent =
                        _scheduledEvents[index];

                    var time = scheduledEvent.Finish - scheduledEvent.Start;
                    return string.Format("{0}", time);
                });


                _scheduleNameFeedbacks.Add(name);
                _scheduleStartFeedbacks.Add(start);
                _scheduleEndFeedbacks.Add(end);
                _scheduleIdFeedbacks.Add(id);
                _scheduleLengthFeedbacks.Add(length);
            }

            _runningEventNameFeedback =
                new StringFeedback(() => _runningEvent != null ? _runningEvent.Title : string.Empty);
            _runningEventStartFeedback =
                new StringFeedback(() => _runningEvent != null ? _runningEvent.Start.ToLocalTime().ToString("hh:mm:ss tt") : string.Empty);
            _runningEventEndFeedback =
                new StringFeedback(() => _runningEvent != null ? _runningEvent.Finish.ToLocalTime().ToString("hh:mm:ss tt") : string.Empty);
            _runningEventIdFeedback = new StringFeedback(() => _runningEvent != null ? _runningEvent.Id : string.Empty);
            _runningEventLengthFeedback = new StringFeedback(() =>
            {
                if (_runningEvent == null)
                {
                    return string.Empty;
                }

                var length = _runningEvent.Finish - _runningEvent.Start;

                return string.Format("{0}", length);
            });

            _runningEventRunningFeedback =
                new BoolFeedback(
                    () => _runningEvent != null && _runningEvent.Status.Equals(RunningStatus, StringComparison.InvariantCultureIgnoreCase));

            _nextEventExistsFeedback = new BoolFeedback(() => _scheduledEvents.Count > 0);
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new EpiphanPearlJoinMap(joinStart);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }

            trilist.SetSigTrueAction(joinMap.Start.JoinNumber, StartEvent);
            trilist.SetSigTrueAction(joinMap.Stop.JoinNumber, StopRunningEvent);
            trilist.SetSigTrueAction(joinMap.Pause.JoinNumber, PauseRunningEvent);
            trilist.SetSigTrueAction(joinMap.Resume.JoinNumber, ResumeRunningEvent);
            trilist.SetSigTrueAction(joinMap.Extend.JoinNumber, ExtendRunningEvent);

            CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RecorderOnline.JoinNumber]);

            _runningEventRunningFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsRecording.JoinNumber]);
            _runningEventRunningFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.IsPaused.JoinNumber]);

            _runningEventNameFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingName.JoinNumber]);
            _runningEventStartFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingStartTime.JoinNumber]);
            _runningEventEndFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingEndTime.JoinNumber]);
            _runningEventIdFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingId.JoinNumber]);
            _runningEventLengthFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingLength.JoinNumber]);

            _scheduleNameFeedbacks[0].LinkInputSig(trilist.StringInput[joinMap.NextRecordingName.JoinNumber]);
            _scheduleStartFeedbacks[0].LinkInputSig(trilist.StringInput[joinMap.NextRecordingStartTime.JoinNumber]);
            _scheduleEndFeedbacks[0].LinkInputSig(trilist.StringInput[joinMap.NextRecordingEndTime.JoinNumber]);
            _scheduleIdFeedbacks[0].LinkInputSig(trilist.StringInput[joinMap.NextRecordingId.JoinNumber]);
            _scheduleLengthFeedbacks[0].LinkInputSig(trilist.StringInput[joinMap.NextRecordingLength.JoinNumber]);

            _nextEventExistsFeedback.LinkInputSig(trilist.BooleanInput[joinMap.NextRecordingExists.JoinNumber]);

            trilist.OnlineStatusChange += (device, args) =>
            {
                if (!args.DeviceOnLine) return;

                trilist.StringInput[joinMap.CurrentRecordingId.JoinNumber].StringValue = _runningEventIdFeedback.StringValue;
                trilist.StringInput[joinMap.CurrentRecordingName.JoinNumber].StringValue = _runningEventNameFeedback.StringValue;
                trilist.StringInput[joinMap.CurrentRecordingStartTime.JoinNumber].StringValue = _runningEventStartFeedback.StringValue;
                trilist.StringInput[joinMap.CurrentRecordingEndTime.JoinNumber].StringValue = _runningEventEndFeedback.StringValue;
                trilist.StringInput[joinMap.CurrentRecordingLength.JoinNumber].StringValue = _runningEventLengthFeedback.StringValue;

                Debug.Console(2, this, "Bridge online.");

                Debug.Console(2, this, "{0} - {1} | {2} | {3} | {4} | {5}", 0, _scheduleIdFeedbacks[0].StringValue, _scheduleNameFeedbacks[0].StringValue, _scheduleStartFeedbacks[0].StringValue, _scheduleEndFeedbacks[0].StringValue, _scheduleLengthFeedbacks[0].StringValue);

                trilist.StringInput[joinMap.NextRecordingId.JoinNumber].StringValue = _scheduleIdFeedbacks[0].StringValue;
                trilist.StringInput[joinMap.NextRecordingName.JoinNumber].StringValue = _scheduleNameFeedbacks[0].StringValue;
                trilist.StringInput[joinMap.NextRecordingStartTime.JoinNumber].StringValue = _scheduleStartFeedbacks[0].StringValue;
                trilist.StringInput[joinMap.NextRecordingEndTime.JoinNumber].StringValue = _scheduleEndFeedbacks[0].StringValue;
                trilist.StringInput[joinMap.NextRecordingLength.JoinNumber].StringValue = _scheduleLengthFeedbacks[0].StringValue;
            };
        }

        private void StartEventStatusTimer()
        {
            if (_statusTimer != null)
            {
                _statusTimer.Stop();
                _statusTimer.Dispose();
                _statusTimer = null;
            }

            _statusTimer = new CTimer(o => GetRunningEventStatus(), null, 0, 5000);
        }

        private void StopEventStatusTimer()
        {
            if (_statusTimer == null) return;

            _statusTimer.Stop();
            _statusTimer.Dispose();
            _statusTimer = null;
        }

        private void PauseRunningEvent()
        {
            if (_runningEvent == null)
            {
                Debug.Console(0, this, "No running event");
                return;
            }

            var path = string.Format("/schedule/events/{0}/control/pause", _runningEvent.Id);

            var response = _client.Post<ScheduleResponse<string>>(path);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to pause event");

                _monitor.SetOnlineStatus(false);

                return;
            }

            if (!response.Status.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                Debug.Console(0, this, "Error pausing event: {0}", response.Message);
            }
        }

        private void ResumeRunningEvent()
        {
            if (_runningEvent == null)
            {
                Debug.Console(0, this, "No running event");
                return;
            }

            var path = string.Format("/schedule/events/{0}/control/resume", _runningEvent.Id);

            var response = _client.Post<ScheduleResponse<string>>(path);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to resume event");

                _monitor.SetOnlineStatus(false);

                return;
            }

            if (!response.Status.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                Debug.Console(0, this, "Error resuming event: {0}", response.Message);
            }
        }

        private void StopRunningEvent()
        {
            if (_runningEvent == null)
            {
                Debug.Console(0, this, "No running event");
                return;
            }

            var path = string.Format("/schedule/events/{0}/control/stop", _runningEvent.Id);

            var response = _client.Post<ScheduleResponse<string>>(path);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to stop event");

                _monitor.SetOnlineStatus(false);

                return;
            }

            if (!response.Status.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                Debug.Console(0, this, "Error stopping event: {0}", response.Message);
            }

            StopEventStatusTimer();

            GetRunningEventStatus();

            GetRunningEvent();
        }

        private void StartEvent()
        {
            var id = string.Empty;
            if (_scheduledEvents.Count > 0)
            {
                id = _scheduledEvents[0].Id;
            }

            if (string.IsNullOrEmpty(id))
            {
                Debug.Console(0, this, "No scheduled event to start");
                return;
            }

            var path = string.Format("/schedule/events/{0}/control/stop", id);

            var response = _client.Post<ScheduleResponse<string>>(path);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to start event");

                _monitor.SetOnlineStatus(false);

                return;
            }

            if (!response.Status.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                Debug.Console(0, this, "Error starting event: {0}", response.Message);
                return;
            }

            GetRunningEvent();

            StartEventStatusTimer();
        }

        private void ExtendRunningEvent()
        {
            var path = string.Format("/schedule/events/{0}/control/extend", _runningEvent.Id);

            var body = new ExtendEventRequest
            {
                Finish = _runningEvent.Finish + new TimeSpan(0, 0, 15, 0)
            };

            var response = _client.Post<ExtendEventRequest, ScheduleResponse<String>>(path, body);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to extend event");

                _monitor.SetOnlineStatus(false);

                return;
            }

            if (!response.Status.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                Debug.Console(0, this, "Error extending event: {0}", response.Message);
            }
        }

        private void GetScheduledEvents()
        {
            var from = DateTime.Now.Date;

            var to = from + new TimeSpan(1, 0, 0, 0);

            var todayScheduledPath = string.Format("/schedule/events/?from={0}&to={1}&status=scheduled", from, to);

            var response = _client.Get<ScheduleResponse<List<Event>>>(todayScheduledPath);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to get scheduled events");

                _scheduledEvents = new List<Event>();

                UpdateFeedbacks();

                _monitor.SetOnlineStatus(false);

                return;
            }

            _monitor.SetOnlineStatus(true);

            _scheduledEvents = response.Result;

            Debug.Console(2, this, "Scheduled Events");

            for (var i = 0; i < _scheduledEvents.Count; i++)
            {
                Debug.Console(2, this, "{0} - {1} | {2} | {3} | {4}", i, _scheduledEvents[i].Id, _scheduledEvents[i].Title, _scheduledEvents[i].Start, _scheduledEvents[i].Finish);
            }

            UpdateFeedbacks();
        }

        private void GetRunningEvent()
        {
            // Current event could be either running or paused

            Debug.Console(2, this, "Getting Running Events");

            var runningEventPath = string.Format("/schedule/events/?status=running");

            var pausedEventPath = string.Format("/schedule/events/?status=paused");

            var response = _client.Get<ScheduleResponse<List<Event>>>(runningEventPath);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to get running event");
            }

            if (response != null && response.Result.Count > 0)
            {
                _runningEvent = response.Result[0];

                Debug.Console(2, this, "Running Event: {0} | {1} | {2} | {3} | ", _runningEvent.Id,
                    _runningEvent.Title, _runningEvent.Start, _runningEvent.Finish);

                UpdateFeedbacks();

                StartEventStatusTimer();
                return;
            }

            response = _client.Get<ScheduleResponse<List<Event>>>(pausedEventPath);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to get paused event");
                _runningEvent = null;

                UpdateFeedbacks();
                StopEventStatusTimer();

                return;
            }

            _runningEvent = response.Result.Count > 0 ? response.Result[0] : null;

            UpdateFeedbacks();

            if (_runningEvent != null)
            {
                StartEventStatusTimer();

                Debug.Console(2, this, "Running Event: {0} | {1} | {2} | {3} | ", _runningEvent.Id,
                    _runningEvent.Title, _runningEvent.Start, _runningEvent.Finish);
            }
            else
            {
                StopEventStatusTimer();
            }
        }

        private void GetRunningEventStatus()
        {
            if (_runningEvent == null)
            {
                Debug.Console(1, this, "No Running Event");
                return;
            }

            var path = string.Format("/schedule/events/{0}/status", _runningEvent.Id);

            var response = _client.Get<ScheduleResponse<string>>(path);

            if (response == null)
            {
                Debug.Console(0, this, "Unable to get running event status");
                return;
            }

            _runningEvent.Status = response.Result;

            _runningEventRunningFeedback.FireUpdate();
        }

        private void UpdateFeedbacks()
        {
            _runningEventNameFeedback.FireUpdate();
            _runningEventStartFeedback.FireUpdate();
            _runningEventEndFeedback.FireUpdate();
            _runningEventIdFeedback.FireUpdate();
            _runningEventLengthFeedback.FireUpdate();

            _runningEventRunningFeedback.FireUpdate();
            _nextEventExistsFeedback.FireUpdate();

            for (var i = 0; i < _scheduledEvents.Count; i++)
            {
                _scheduleNameFeedbacks[i].FireUpdate();
                _scheduleStartFeedbacks[i].FireUpdate();
                _scheduleEndFeedbacks[i].FireUpdate();
                _scheduleIdFeedbacks[i].FireUpdate();
                _scheduleLengthFeedbacks[i].FireUpdate();
            }
        }
    }
}