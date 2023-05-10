using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;
using CommandName = Crestron.Panopto.Command.CommandName;
using CommandType = Crestron.Panopto.Command.CommandType;

namespace Crestron.Panopto
{
    public class PausedState : PanoptoState
    {
        protected CTimer SessionPollingTimer;
        protected CTimer RecorderPollingTimer;
        private RecorderState _recorderState = RecorderState.Unknown;
        private SessionState _sessionState = SessionState.Unknown;
        private bool _recordingStopped = false;

        public PausedState(Panopto.Driver p)
        {
            P = p;
            PanoptoLogger.Notice("Panopto.PausedState recordingId is {0}", p.RecordingConfig.RecordingId);
            RecorderPollingTimer = new CTimer(CheckRecorderStatus, Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
            SessionPollingTimer = new CTimer(CheckSessionStatus, null, Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
        }

        public override string GetStateName()
        {
            return StateConsts.PauseState;
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

            //in debug this will be all zeros so it will be easy to know if the driver enters the pause state without being assigned the pause guid
            P.RecordingConfig.PauseId = new Guid();
        }

        public override void HttpDataHandler(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.PausedState.HttpDataHandler received response from Panopto.");
            
            //resume doesn't have a traditional response message like all the other commands. We just get an response code 200
            //however since the state is also polling the remote recorder and session states it needs to handle the response message for those.
            if (!string.IsNullOrEmpty(args.ResponseString))
            {
                //not a resume response so figure out what it is
                PanoptoLogger.Notice(args.ResponseString);
                if (args.ResponseString.Contains("<GetSessionsByIdResponse"))
                {
                    ProcessGetSessionsByIdResponse(args);
                }
                else if (args.ResponseString.Contains("Invalid Session Id ") && args.ResponseString.Contains("The current user does not have access to call this method."))
                {
                    PanoptoLogger.Notice("Invalid session id. This typically means that the session no longer exists, so it isn't recording.");
                    GoToIdleState();
                }
                else if (args.ResponseString.Contains("<faultstring xml:lang=\"en-US\">\"Unable to update a completed recording"))
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
            }
            else
            {
                //no message check for a 200 response code
                Command command = args.Command;
                switch (args.ResponseCode)
                {
                    case 200:
                        {
                            if (!_recordingStopped)
                            {
                                PanoptoLogger.Notice("Ack message received. Assuming recording is resumed.");
                                PanoptoSession sessionInfo = new PanoptoSession()
                                {
                                    Name = P.RecordingConfig.RecordingName,
                                    Duration = P.RecordingConfig.Duration,
                                    StartTime = P.RecordingConfig.StartTime,
                                    IsBroadcast = P.RecordingConfig.IsBroadcast
                                };
                                PanoptoLogger.Notice("recordingId is {0}", P.RecordingConfig.RecordingId);
                                ChangeState(new RecordingState(P), false, DisplayMessageEnum.Resuming, sessionInfo);
                                command.Ready = true;
                            }
                            break;
                        }
                    default:
                        {
                            command.Ignore = true;
                            break;
                        }
                }
                P.SendResult(command);
            }
        }

        private void ProcessGetRemoteRecorderByIdResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.PausedState.ProcessGetRemoteRecorderByIdResponse");
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

        private void ProcessGetSessionsByIdResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.PausedState.ProcessGetSessionsByIdResponse");
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
            PanoptoLogger.Notice("Panopto.PausedState.SendSessionFeedback");
            PanoptoFeedback pf = new PanoptoFeedback()
            {
                Transitioning = false,
                FeedbackMessageValue = DisplayMessageEnum.Recording,
                SessionInfo = P.NextSession
            };
            P.ChangeFeedback(pf);
        }

        protected void CheckSessionStatus(object stateInfo)
        {
            PanoptoLogger.Notice("Panopto.PausedState.CheckSessionStatus recordingId is {0}", P.RecordingConfig.RecordingId);
            P.API.GetSessionById(P, CommandType.Poll, this);
        }

        protected void CheckRecorderStatus(object stateInfo)
        {
            PanoptoLogger.Notice("Panopto.PausedState.CheckRecorderStatus");
            P.API.GetRemoteRecorderById(P, CommandType.Poll, this);
        }

        private void GoToIdleState()
        {
            PanoptoLogger.Notice("Panopto.PausedState.GotoIdleState");
            ChangeState(new IdleState(P), false, DisplayMessageEnum.Processing);
            P.RecordingConfig = null;
        }

        public override void Resume()
        {
            PanoptoLogger.Notice("Panopto.PausedState.Resume");
            var pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.Resuming
            };
            P.ChangeFeedback(pf);
            P.API.Resume(P, CommandType.Control, this);
        }

        public override void Stop()
        {
            PanoptoLogger.Notice("Panopto.PausedState.Stop");
            P.RecordingConfig.EndTime = DateTime.Now;
            P.API.StopRecording(P, CommandType.Control, this);
            _recordingStopped = true;
        }
    }
}