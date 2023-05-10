using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;
using Crestron.Panopto.Common;
using Crestron.Panopto.Common.Transports;
using Crestron.Panopto;
using Crestron.SimplSharp.Net.Https;
using CommandType = Crestron.Panopto.Command.CommandType;

namespace Crestron.Panopto
{
    public class Driver
    {
        public PreviewData PreviewData { get; set; }
        private PanoptoState _state;
        public PanoptoFeedback Feedback;
        public ConnectionFeedback Connection;
        public Commands API = new Commands();
        public string Cookie;

        private PanoptoSession _nextSession;
        public PanoptoSession NextSession
        {
            get
            {
                return _nextSession;
            }
            set
            {
                _nextSession = value;
                PanoptoLogger.Notice("Setting NextSession.Name to {0} StartTime to {1} Duration to {2}", NextSession.Name, NextSession.StartTime, NextSession.Duration);
            }
        }

        public const int PollingInterval = 7000;
        public const int PollingTimeoutDueTime = 45000;
        public const int PollingDueTime = 1000;
        
        public int RefreshRate = 5000;
        private int _minRefreshRate = 5000;

        //This Guid is derived from the first guid in the sessions array in the GetRemoteRecorderByIdResponse
        //This Guid is used to search for the session using the GetSessionsById command
        public Guid ReportedSessionId;

        public string RecorderId;
        public string PanoptoUrl = string.Empty;
        public string UserName = string.Empty;
        public string Password = string.Empty;
        public string RecorderName = string.Empty;
        public string IpAddress = string.Empty;
        public RecordingConfig RecordingConfig = new RecordingConfig();
        private CTimer _transitioningTimeoutTimer;

        public CCriticalSection StateChangeLock = new CCriticalSection();

        HttpTransport httpTransport;

        public Driver()
        {
            Observers = new List<IObserver>();
            _state = new InitialState(this, new CrestronDataStoreWrapper());

            httpTransport = new HttpTransport(this);
            httpTransport.HttpDataHandler += HttpDataHandler;
            httpTransport.HttpDownloadDataHandler += HttpDownloadDataHandler;

            _transitioningTimeoutTimer = new CTimer(TransitioningTimedOut, PollingDueTime);
            _transitioningTimeoutTimer.Stop();

            PanoptoLogger.PrepareLogs();
        }

        public void RescheduledSessionResult(bool result, PanoptoSession newSession)
        {
            PanoptoLogger.Notice("Panopto.RescheduledSessionResult Notifying observers");
            foreach (var observer in Observers)
            {
                observer.RescheduleConflictResult(result, newSession);
            }
        }

        public bool DiagnosticLoggingEnabled
        {
            get
            {
                return PanoptoLogger.DiagnosticLoggingEnabled;
            }
            set
            {
                PanoptoLogger.DiagnosticLoggingEnabled = value;
            }
        }

        public void ChangeState(PanoptoState state, bool transitioning, DisplayMessageEnum message, PanoptoSession sessionInfo)
        {
            PanoptoLogger.Notice("Panopto.ChangeState");
            PanoptoFeedback pf = null;
            try
            {
                StateChangeLock.Enter();
                if (state != null)
                {
                    PanoptoLogger.Notice("Changing to state {0}", state.GetStateName());

                    if (!state.GetStateName().Equals(StateConsts.InitialState))
                    {
                        this.PanoptoUrl = state.P.PanoptoUrl;
                        this.UserName = state.P.UserName;
                        this.Password = state.P.Password;
                        this.RecorderName = state.P.RecorderName;
                        this.IpAddress = state.P.IpAddress;
                        this.RecorderId = state.P.RecorderId;
                    }

                    PanoptoLogger.Notice("Stopping previous state");
                    _state.StopState();
                    ClearQueue();
                    _state = state;

                    pf = new PanoptoFeedback()
                    {
                        Transitioning = transitioning,
                        FeedbackMessageValue = message,
                        DriverState = GetRoomStateName()
                    };

                    if (sessionInfo is PanoptoSession)
                    {
                        pf.SessionInfo = sessionInfo;
                    }
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.ChangeState Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
            finally
            {
                StateChangeLock.Leave();
            }

            if (pf is PanoptoFeedback)
            {
                ChangeFeedback(pf);
            }
        }

        public void ChangeFeedback(PanoptoFeedback feedback)
        {
            PanoptoLogger.Notice("Panopto.ChangeFeedback");

            if (_state is InitialState)
            {
                PanoptoLogger.Notice("Not applying loading timeout to the inital state for security reasons");
            }
            else if (feedback.Transitioning != null)
            {
                if ((bool)feedback.Transitioning)
                {
                    _transitioningTimeoutTimer.Reset(PollingTimeoutDueTime);
                }
                else
                {
                    _transitioningTimeoutTimer.Stop();
                }
            }
            else
            {
                PanoptoLogger.Notice("feedback.Transitioning unused");
            }

            this.Feedback = feedback;
            FeedbackNotify();
        }

        private void TransitioningTimedOut(object userspecific)
        {
            ChangeState(new IdleState(this), false, DisplayMessageEnum.AbleToAccessRemoteRecorder, null);
        }

        public void ChangeConnection(ConnectionFeedback feedback)
        {
            PanoptoLogger.Notice("Panopto.ChangeConnection");
            this.Feedback.ServerDisconnect = !feedback.State;
            this.Connection = feedback;
            ConnectionNotify();
        }

        public void Configure(string panoptoUrl, string userName, string password, string recorderName, string ipAddress, string RecorderId, bool transitioning, DisplayMessageEnum message)
        {
            PanoptoLogger.Notice("Panopto.Configure");

            this.PanoptoUrl = panoptoUrl;
            this.UserName = userName;
            this.Password = password;
            this.RecorderName = recorderName;
            this.IpAddress = ipAddress;
            this.RecorderId = RecorderId;

            var pf = new PanoptoFeedback()
            {
                Transitioning = transitioning,
                FeedbackMessageValue = message
            };
            ChangeFeedback(pf);

            _state = new InitialState(this, new CrestronDataStoreWrapper());
            _state.P.PanoptoUrl = panoptoUrl;
            _state.P.UserName = userName;
            _state.P.Password = password;
            _state.P.RecorderName = recorderName;
            _state.P.IpAddress = ipAddress;
            _state.P.RecorderId = RecorderId;

            API.LogOnWithPassword(this, userName, password, panoptoUrl, CommandType.Control, _state);
        }

        public void Reconfigure()
        {
            PanoptoLogger.Notice("Panopto.Reconfigure");
            ChangeState(new InitialState(this, new CrestronDataStoreWrapper(), true), false, DisplayMessageEnum.LoggingIn, null);
            this.RecorderId = string.Empty;
        }

        //the refresh rate is in seconds
        public void SetPreviewRefreshRate(int rate)
        {
            PanoptoLogger.Notice("Panopto.SetPreviewRefreshRate");
            //convert rate into milliseconds
            rate = rate * 1000;
            PanoptoLogger.Notice("rate is {0}", rate);

            if (rate >= _minRefreshRate)
            {
                PanoptoLogger.Notice("Requested refresh rate is greater then or equal to the minimum refresh rate. The driver will use the requested refresh rate");
                RefreshRate = rate;
            }
            else
            {
                PanoptoLogger.Notice("Requested refresh rate is not greater then or equal to the minimum refresh rate. The driver will use the minimum refresh rate");
                RefreshRate = _minRefreshRate;
            }
            _state.SetPreviewRefreshRate(RefreshRate);
        }

        public void ScheduleRecording(string recordingName, bool isBroadcast, DateTime startTime, double duration)
        {
            PanoptoLogger.Notice("ScheduleRecording duration = {0} state is {1}", duration, _state.GetStateName());
            _state.Record(recordingName, startTime, duration, isBroadcast);
        }

        public void SetCurrentPage(CurrentPage page)
        {
            PageNotify(page);
        }

        public void RescheduleRecording(Guid sessionId, DateTime startTime, DateTime endTime, PanoptoSession newSession)
        {
            PanoptoLogger.Notice("Panopto.RescheduleRecording");
            var pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.SchedulingSession
            };
            ChangeFeedback(pf);
            _state.RescheduleSession(sessionId, startTime, endTime, newSession);
        }

        public void Preview(string recordingName)
        {
            PanoptoLogger.Notice("Panopto.Preview");
            var pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.BeginningPreview
            };
            ChangeFeedback(pf);
            PanoptoLogger.Notice("Feedback changed");
            PanoptoLogger.Notice("Current state is {0}", _state.GetStateName());
            _state.Preview(recordingName);
        }

        public void RecordNow(string recordingName, bool isBroadcast, double duration)
        {
            PanoptoLogger.Notice("Panopto.RecordNow");
            DateTime ScheduledTime = DateTime.UtcNow.ToLocalTime();
            PanoptoLogger.Notice("Duration is {0}", duration);
            ScheduleRecording(recordingName, isBroadcast, ScheduledTime, duration);
        }

        public int RecordUntil(string recordingName, bool isBroadcast)
        {
            PanoptoLogger.Notice("Panopto.RecordUntil");
            PanoptoLogger.Notice("State is {0}", _state.GetStateName());
            return _state.RecordUntil(recordingName, isBroadcast);
        }

        public void StartRecordingEarly()
        {
            PanoptoLogger.Notice("Panopto.StartRecordingEarly");
            var pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.StartingSessionEarly
            };
            ChangeFeedback(pf);
            _state.RecordEarly();
        }

        public void NewRecording()
        {
            _state.NewRecording();
        }

        public void Back()
        {
            _state.Back();
        }

        public void StopRecording()
        {
            PanoptoLogger.Notice("Panopto.StopRecording");
            var pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.Stopping
            };
            ChangeFeedback(pf);
            _state.Stop();
        }

        public void Pause()
        {
            PanoptoLogger.Notice("Panopto.Pause");
            var pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.BeginningPause
            };
            ChangeFeedback(pf);
            _state.Pause();
        }

        public void Resume()
        {
            PanoptoLogger.Notice("Panopto.Resume");
            var pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.Resuming
            };
            ChangeFeedback(pf);
            _state.Resume();
        }

        public void ExtendRecording()
        {
            PanoptoLogger.Notice("Panopto.ExtendRecording");
            var pf = new PanoptoFeedback() { FeedbackMessageValue = DisplayMessageEnum.ExtendingSession };
            ChangeFeedback(pf);
            _state.Extend();
        }

        public List<PanoptoSession> GetUpcomingSessions()
        {
            PanoptoLogger.Notice("Panopto.GetUpcomingSessions");
            List<PanoptoSession> sessions = new List<PanoptoSession>();
            sessions = _state.GetUpcomingRecordings();
            return sessions;
        }

        public void ClearQueue()
        {
            PanoptoLogger.Notice("Panopto.ClearQueue");
            httpTransport.ClearQueue();
        }

        public void QueueMessage(Command command)
        {
            PanoptoLogger.Notice("Panopto.QueueMessage");
            httpTransport.QueueMessage(command);
        }

        public void SendResult(Command command)
        {
            PanoptoLogger.Notice("Panopto.SendResult");
            httpTransport.ResultReceived(command);
        }

        private void HttpDownloadDataHandler(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.HttpDownloadDataHandler");
            if (_state == args.Command.State)
            {
                PanoptoLogger.Notice("the state matches so passing the response to the state response validation");
                _state.HttpDownloadDataHandler(args);
            }
            else
            {
                PanoptoLogger.Notice("the state doesn't match so ignoring the response");
            }
        }

        private void HttpDataHandler(HttpsResponseArgs args)
        {
            try
            {
                PanoptoLogger.Notice("Panopto.HttpDataHandler");
                if (_state is PanoptoState)
                {
                    if (_state == args.Command.State)
                    {
                        PanoptoLogger.Notice("the state matches so passing the response to the state response validation");
                        _state.HttpDataHandler(args);
                    }
                    else
                    {
                        PanoptoLogger.Notice("the state doesn't match so ignoring the response");
                        PanoptoLogger.Notice("state is {0} command.State is {1}", _state.GetStateName(), args.Command.State.GetStateName());
                        Command command = args.Command;
                        command.Ignore = true;
                        SendResult(command);
                    }
                }
                else
                {
                    PanoptoLogger.Notice("Panopto.HttpDataHandler _state is not a instance of PanoptoState. Ignoring a possible interstate response.");
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.HttpDataHandler Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
                PanoptoLogger.Error("stacktrace: {0}", e.StackTrace);
            }
        }

        #region ABaseDynamicDriver Members

        public void Dispose()
        {
        }

        public bool SupportsFeedback
        {
            get { return true; }
        }

        public string Description
        {
            get { return "Panopto API v4.2"; }
        }

        public Guid Guid
        {
            get { return new Guid("818b23b3-59d6-46b6-b8ac-295e558bfdd0"); }
        }

        public string Manufacturer
        {
            get { return "Panopto"; }
        }

        public DateTime VersionDate
        {
            get { return DateTime.Now; }
        }

        public string BaseModel
        {
            get { return string.Empty; }
        }

        public string DriverVersion
        {
            get { return "1.0.0.0"; }
        }

        public List<string> SupportedSeries
        {
            get { return new List<string>(); }
        }

        public List<string> SupportedModels
        {
            get { return new List<string>() { string.Empty }; }
        }

        #endregion

        #region ISubject Members
        public List<IObserver> Observers { get; set; }

        private static CMutex lockIt = new CMutex();

        public PanoptoState GetRoomState()
        {
            PanoptoLogger.Notice("Panopto.GetRoomState");
            return _state;
        }

        public string GetRoomStateName()
        {
            string state = string.Empty;
            state = _state.GetStateName();
            PanoptoLogger.Notice("Panopto.GetRoomState state is {0}", state);
            return state;
        }

        public void Register(IObserver observer)
        {
            lockIt.WaitForMutex();
            try
            {
                if (!Observers.Contains(observer))
                {
                    Observers.Add(observer);
                    if (GetRoomStateName() == StateConsts.InitialState)
                    {
                        //Creating new InitialState here because we have to wait for registration
                        //If we retrieve stored configuration in constructor, we don't have observers
                        //so the UI and driver get out of sync
                        ChangeState(new InitialState(this, new CrestronDataStoreWrapper()), false, DisplayMessageEnum.LoggingIn, null);
                        //_state.TryAutoConfigure();
                    }
                }
            }
            catch (Exception ex)
            {
                PanoptoLogger.Error("Panopto.Register Error: {0}", ex.Message);
            }
            finally
            {
                lockIt.ReleaseMutex();
            }
        }

        public void Unregister(IObserver observer)
        {
            if (Observers.Contains(observer))
            {
                Observers.Remove(observer);
            }
        }

        public void ConnectionNotify()
        {
            PanoptoLogger.Notice("Connection Notifying observers");
            foreach (var observer in Observers)
            {
                observer.Update(this.Connection);
            }
        }

        public void PageNotify(CurrentPage page)
        {
            PanoptoLogger.Notice("Page Notifying observers");
            foreach (var observer in Observers)
            {
                observer.Update(page);
            }
        }

        public void FeedbackNotify()
        {
            PanoptoLogger.Notice("Feedback Notifying observers");
            foreach (var observer in Observers)
            {
                observer.Update(this.Feedback);
            }
        }
        #endregion
    }
}