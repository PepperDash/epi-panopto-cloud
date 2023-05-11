using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;
using Newtonsoft.Json;
using CommandName = Crestron.Panopto.Command.CommandName;
using CommandType = Crestron.Panopto.Command.CommandType;

namespace Crestron.Panopto
{
    public class RecordingState : PanoptoState
    {
        private RecorderState _recorderState = RecorderState.Unknown;
        private SessionState _sessionState = SessionState.Unknown;
        protected CTimer SessionPollingTimer;
        protected CTimer RecorderPollingTimer;
        private bool _recordingStopped = false;


        public RecordingState(Panopto.Driver p)
        {
            P = p;
            PanoptoLogger.Notice("Panopto.RecordingState");
            if (!string.IsNullOrEmpty(P.RecorderId))
            {
                PanoptoLogger.Notice("GUID is {0}", P.RecorderId);
                RecorderPollingTimer = new CTimer(CheckRecorderStatus, Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
            }
            
            if (P.NextSession == null)
            {
                P.NextSession = new PanoptoSession();
            }
            P.NextSession.Name = P.RecordingConfig.RecordingName;
            P.NextSession.Duration = P.RecordingConfig.Duration;
            P.NextSession.StartTime = P.RecordingConfig.StartTime;
            P.NextSession.IsBroadcast = P.RecordingConfig.IsBroadcast;
            P.NextSession.RecordingId = P.RecordingConfig.RecordingId;
            
            SendSessionFeedback();
            SessionPollingTimer = new CTimer(CheckSessionStatus, null, Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
        }

        public override string GetStateName()
        {
            return StateConsts.RecordingState;
        }

        public override void StopState()
        {
            PanoptoLogger.Notice("Panopto.RecordingState.StopState");
            if (!SessionPollingTimer.Disposed)
            {
                SessionPollingTimer.Stop();
                SessionPollingTimer.Dispose();
            }
            if (!RecorderPollingTimer.Disposed)
            {
                RecorderPollingTimer.Stop();
                RecorderPollingTimer.Dispose();
            }
        }

        public override void HttpDataHandler(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.RecordingState.HttpDataHandler received response from Panopto");
            if (!string.IsNullOrEmpty(args.ResponseString))
            {
                if (args.ResponseString.Contains("<UpdateRecordingTimeResponse"))
                {
                    ProcessUpdateRecordingTimeResponse(args);
                }
                else if (args.ResponseString.Contains("<GetSessionsByIdResponse"))
                {
                    ProcessGetSessionsByIdResponse(args);
                }
                else if (args.ResponseString.Contains("SessionPublicID"))
                {
                    ProcessGetPublicSessionIdResponse(args);
                }
                else if (args.ResponseString.Contains("Invalid Session Id ") && args.ResponseString.Contains("The current user does not have access to call this method."))
                {
                    PanoptoLogger.Notice("Invalid session id. This typically means that the session no longer exists, so it isn't recording.");
                    GoToIdleState();
                }
                else if (args.ResponseString.Contains("Unable to update a completed recording"))
                {
                    GoToIdleState();
                }
                else if (args.ResponseString.Contains("<GetRemoteRecordersByIdResponse"))
                {
                    ProcessGetRemoteRecorderByIdResponse(args);
                }
                else if (args.ResponseString.ToLower().Contains("fault"))
                {
                    GoToIdleState();
                }
                else
                {
                    ProcessPauseResponse(args);
                }
            }
        }

        private void ProcessGetRemoteRecorderByIdResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.RecordingState.ProcessGetRemoteRecorderByIdResponse");
            Command command = args.Command;
            command.Ready = true;
            if (args.ResponseString.Contains("<a:State>"))
            {
                _recorderState = StateHelper.GetRemoteRecorderState(args.ResponseString);
            }
            P.ReportedSessionId = StateHelper.GetNextSessionGuid(args.ResponseString);
            P.SendResult(command);
            CheckRecorderSessionState();
        }

        private void ProcessUpdateRecordingTimeResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.RecordingState.ProcessUpdateRecordingTimeResponse commandName.{0}", args.Command.Name);
            Command command = args.Command;
            command.Ready = true;
            if (args.ResponseString.Contains("Fault") || args.ResponseString.Contains("<a:ConflictsExist>true</a:ConflictsExist>"))
            {
                PanoptoFeedback pf = new PanoptoFeedback()
                {
                    Transitioning = false,
                    FeedbackMessageValue = DisplayMessageEnum.UnableToStop
                };
                P.ChangeFeedback(pf);
            }
            else
            {
                switch (args.Command.Name)
                {
                    case CommandName.Extend:
                        {
                            PanoptoSession sessionInfo = new PanoptoSession()
                            {
                                Name = P.RecordingConfig.RecordingName,
                                Duration = P.RecordingConfig.Duration,
                                StartTime = P.RecordingConfig.StartTime,
                                IsBroadcast = P.RecordingConfig.IsBroadcast
                            };
                            PanoptoFeedback pf = new PanoptoFeedback()
                            {
                                SessionInfo = sessionInfo,
                                Transitioning = false,
                                FeedbackMessageValue = DisplayMessageEnum.Recording
                            };
                            P.ChangeFeedback(pf);
                            break;
                        }
                    case CommandName.Stop:
                        {
                            break;
                        }
                }
            }
            P.SendResult(command);
        }

        private void ProcessGetSessionsByIdResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.RecordingState.ProcessGetSessionsByIdResponse");
            Command command = args.Command;
            try
            {
                P.NextSession = StateHelper.ProcessNextSessionByRecordingId(args.ResponseString, P.ReportedSessionId, P);
                PanoptoLogger.Notice("P.NextSession.Name is {0} P.NextSession.StartTime is {1} P.NextSession.Duration is {2}", P.NextSession.Name, P.NextSession.StartTime, P.NextSession.Duration);
                _sessionState = StateHelper.GetSessionState(args.ResponseString);
                PanoptoLogger.Notice("_sessionState is {0}", _sessionState);
                command.Ready = true;
            }
            catch (Exception ex)
            {
                PanoptoLogger.Error("Panopto.RecordingState.ProcessGetSessionByIdResponse Error message is {0}", ex.Message);
                command.Ignore = true;
            }
            P.SendResult(command);
            CheckRecorderSessionState();
        }

        private void ProcessGetPublicSessionIdResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.RecordingState.ProcessGetPublicSessionIdResponse");
            Command command = args.Command;
            SessionMetadata sessionData = JsonConvert.DeserializeObject<SessionMetadata>(args.ResponseString);
            if (sessionData is SessionMetadata)
            {
                PanoptoLogger.Notice("sesionData.PublicID is '{0}'", sessionData.SessionPublicID);
                P.RecordingConfig.PublicSessionId = new Guid(sessionData.SessionPublicID);
                P.API.Pause(P, CommandType.Control, this);
                command.Ready = true;
            }
            else
            {
                //no session data is possible session no longer exists
                command.Ignore = true;
                GoToIdleState();
            }
            P.SendResult(command);
        }

        private void ProcessPauseResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.RecordingState.ProcessPauseResponse");
            Command command = args.Command;
            if (args.ResponseString.Contains("Not Found"))
            {
                command.Ignore = true;
            }
            else
            {
                command.Ready = true;
                args.ResponseString = args.ResponseString.Replace("\"", "");
                PanoptoLogger.Notice("pauseId is '{0}'", args.ResponseString);
                P.RecordingConfig.PauseId = new Guid(args.ResponseString);
                ChangeState(new PausedState(P), false, DisplayMessageEnum.BeginningPause, null);
            }
            P.SendResult(command);
        }

        protected void CheckSessionStatus(object stateInfo)
        {
            PanoptoLogger.Notice("Panopto.RecordingState.CheckSessionStatus recordingId is {0}", P.RecordingConfig.RecordingId);
            P.API.GetSessionById(P, CommandType.Poll, this);
        }

        protected void CheckRecorderStatus(object stateInfo)
        {
            PanoptoLogger.Notice("Panopto.RecordingState.CheckRecorderStatus");
            P.API.GetRemoteRecorderById(P, CommandType.Poll, this);
        }

        private void CheckRecorderSessionState()
        {
            PanoptoLogger.Notice("Panopto.RecordingState.CheckRecorderSessionState");
            bool recorderIdle = false;
            bool sessionIdle = false;

            if (_recorderState == RecorderState.Previewing)
            {
                recorderIdle = true;
            }

            PanoptoLogger.Notice("recorderIdle is {0}", recorderIdle);
            PanoptoLogger.Notice("_sessionState is {0}", _sessionState.ToString());

            switch (_sessionState)
            {
                case SessionState.Processing:
                    {
                        PanoptoLogger.Notice("Setting feedback to processing");
                        PanoptoFeedback pf = new PanoptoFeedback()
                        {
                            Transitioning = true,
                            FeedbackMessageValue = DisplayMessageEnum.Processing
                        };
                        P.ChangeFeedback(pf);
                        break;
                    }
                case SessionState.Uploading:
                    {
                        PanoptoLogger.Notice("Setting feedback to uploading");
                        PanoptoFeedback pf = new PanoptoFeedback()
                        {
                            Transitioning = true,
                            FeedbackMessageValue = DisplayMessageEnum.Uploading
                        };
                        P.ChangeFeedback(pf);
                        break;
                    }
                case SessionState.Complete:
                case SessionState.Scheduled:
                    {
                        PanoptoLogger.Notice("session is complete or scheduled setting sessionIdle to true");
                        sessionIdle = true;
                        break;
                    }
                case SessionState.Broadcasting:
                case SessionState.Recording:
                    {
                        if (!_recordingStopped)
                        {
                            SendSessionFeedback();
                        }
                        break;
                    }
            }

            PanoptoLogger.Notice("sessionIdle is {0}", sessionIdle);

            if (sessionIdle)
            {
                GoToIdleState();
            }
        }

        private void SendSessionFeedback()
        {
            PanoptoLogger.Notice("Panopto.RecordingState.SendSessionFeedback");
            PanoptoFeedback pf = new PanoptoFeedback()
            {
                Transitioning = false,
                FeedbackMessageValue = DisplayMessageEnum.Recording,
                SessionInfo = P.NextSession
            };
            P.ChangeFeedback(pf);
        }

        public override void Stop()
        {
            PanoptoLogger.Notice("Panopto.RecordingState.Stop");
            P.RecordingConfig.EndTime = DateTime.Now;
            P.API.StopRecording(P, CommandType.Control, this);
            _recordingStopped = true;
        }

        public override void Pause()
        {
            PanoptoLogger.Notice("Panopto.RecordingState.Pause");
            SessionPollingTimer.Stop();
            P.API.GetPublicSessionId(P, CommandType.Control, this);
            P.RecordingConfig.PauseTime = DateTime.Now;
        }

        public override void Extend()
        {
            DateTime originalEndTime = P.RecordingConfig.StartTime.AddSeconds(P.RecordingConfig.Duration);
            DateTime extendedEndTime = originalEndTime.AddMinutes(5);
            P.RecordingConfig.Duration = P.RecordingConfig.Duration + (5 * 60);
            P.RecordingConfig.EndTime = extendedEndTime;
            PanoptoLogger.Notice("About to extend recording to " + extendedEndTime);
            P.API.ExtendRecording(P, CommandType.Control, this);
        }

        private void GoToIdleState()
        {
            PanoptoLogger.Notice("Panopto.RecordingState.GotoIdleState");
            ChangeState(new IdleState(P), false, DisplayMessageEnum.AbleToAccessRemoteRecorder);
            P.RecordingConfig = null;
        }
    }
}