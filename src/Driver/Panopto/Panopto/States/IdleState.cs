using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto;
using Crestron.Panopto.Common.Interfaces;
using Crestron.Panopto.Common.Transports;
using CommandName = Crestron.Panopto.Command.CommandName;
using CommandType = Crestron.Panopto.Command.CommandType;

namespace Crestron.Panopto
{
    public class IdleState : PanoptoState
    {
        private CTimer _schedulePollingTimer;
        private RecorderState _recorderState = RecorderState.Unknown;
        private bool _newRecording = false;
        private bool _early = false;

        public IdleState(Panopto.Driver p)
        {
            PanoptoLogger.Notice("Panopto.IdleState GUID is {0}", p.RecorderId);
            P = p;
            _schedulePollingTimer = new CTimer(CheckSchedule, null, Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
            PanoptoFeedback pf = new PanoptoFeedback()
            {
                Transitioning = false,
                FeedbackMessageValue = DisplayMessageEnum.Unknown
            };
            P.ChangeFeedback(pf);
        }

        public override string GetStateName()
        {
            return StateConsts.IdleState;
        }

        public override void StopState()
        {
            PanoptoLogger.Notice("Panopto.IdleState.StopState");
            if (!_schedulePollingTimer.Disposed)
            {
                _schedulePollingTimer.Stop();
                _schedulePollingTimer.Dispose();
            }
        }

        public override void HttpDataHandler(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.IdleState.HttpDataHandler received response from Panopto");
            if (!string.IsNullOrEmpty(args.ResponseString))
            {
                PanoptoLogger.Notice(args.ResponseString);

                if (args.ResponseString.Contains("<GetRemoteRecordersByIdResponse"))
                {
                    ProcessGetRemoteRecorderByIdResponse(args);
                }
                else if (args.ResponseString.Contains("<GetSessionsByIdResponse"))
                {
                    ProcessGetSessionsByIdResponse(args);
                }
                else if (args.ResponseString.Contains("<GetDefaultFolderForRecorderResponse"))
                {
                    ProcessGetDefaultFolderForRecorderResponse(args);
                }
                else if (args.ResponseString.Contains("<ScheduleRecordingResponse"))
                {
                    ProcessScheduleRecordingResponse(args);
                }
                else if (args.ResponseString.Contains("<UpdateRecordingTimeResponse"))
                {
                    ProcessUpdateRecordingTimeResponse(args);
                }
                else if (args.ResponseString.Contains("Invalid Session Id"))
                {
                    ProcessGetSessionsByIdResponse(args);
                }
                else if (args.ResponseString.Contains("Unable to schedule a recording that ends before it begins"))
                {
                    PanoptoFeedback pf = new PanoptoFeedback()
                    {
                        Transitioning = false,
                        FeedbackMessageValue = DisplayMessageEnum.UnableToScheduleSession
                    };
                    P.ChangeFeedback(pf);
                    PanoptoLogger.Notice("A fault message occured");
                }
            }
        }

        private void ProcessUpdateRecordingTimeResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.IdleState.ProcessUpdateRecordingTimeResponse");
            Command command = args.Command;
            command.Ready = true;
            if (args.ResponseString.Contains("Fault") || args.ResponseString.Contains("<a:ConflictsExist>true</a:ConflictsExist>"))
            {
                PanoptoFeedback pf = new PanoptoFeedback()
                {
                    Transitioning = false,
                    FeedbackMessageValue = DisplayMessageEnum.UnableToStartSessionEarly
                };
                P.ChangeFeedback(pf);
            }
            else
            {
                PanoptoLogger.Notice("args.Command.Name is {0}", args.Command.Name);
                switch (args.Command.Name)
                {
                    case CommandName.StartSessionEarly:
                        {
                            P.RecordingConfig.RecordingId = StateHelper.GetNextSessionGuid(args.ResponseString);
                            P.API.StartSessionEarlyGetSessionById(P, CommandType.Poll, this);
                            PanoptoFeedback pf = new PanoptoFeedback()
                            {
                                Transitioning = true,
                                FeedbackMessageValue = DisplayMessageEnum.StartingSessionEarly
                            };
                            P.ChangeFeedback(pf);
                            command.Ready = true;
                            P.SendResult(command);
                            break;
                        }
                }
            }
        }

        private void ProcessScheduleRecordingResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.IdleState.ProcessScheduleRecordingResponse");
            Command command = args.Command;
            command.Ready = true;
            bool result = StateHelper.ProcessScheduleRecordingResponse(args.ResponseString);
            P.SendResult(command);
        }

        private void ProcessGetDefaultFolderForRecorderResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.IdleState.ProcessGetDefaultFolderForRecorderResponse");
            Command command = args.Command;
            command.Ready = true;
            P.RecordingConfig.FolderId = new Guid(StateHelper.ProcessGetDefaultFolderForRecorderResponse(args.ResponseString));
            PanoptoLogger.Notice("folderGuid is {0}", P.RecordingConfig.FolderId);
            P.API.ScheduleRecording(P, CommandType.Control, this);
            P.SendResult(command);
        }

        private void ProcessGetRemoteRecorderByIdResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.IdleState.ProcessGetRemoteRecorderByIdResponse");
            Command command = args.Command;
            if (args.ResponseString.Contains("<a:ScheduledRecordings xmlns:b=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"/>"))
            {
                PanoptoLogger.Notice("No recordings");
                //depending on where in the polling process the driver is in when the session is deleted
                //from the website it might be here in the get session by id or in GetRemoteRecorderById so
                //be ready to catch the deleted session in both places
                ClearSession();
            }
            else
            {
                PanoptoLogger.Notice("Found scheduled recordings");
                _recorderState = StateHelper.GetRemoteRecorderState(args.ResponseString);
                P.ReportedSessionId = StateHelper.GetNextSessionGuid(args.ResponseString);

                if (P.RecordingConfig == null)
                {
                    PanoptoLogger.Notice("P.RecordingConfig is null. Initializing P.RecordingConfig");
                    P.RecordingConfig = new RecordingConfig();
                }

                PanoptoLogger.Notice("Creating new session");
                if (P.NextSession == null)
                {
                    P.NextSession = new PanoptoSession();
                }

                P.API.GetSessionById(P, CommandType.Control, this);
            }
            command.Ready = true;
            P.SendResult(command);
        }

        //if in the dialog to setup a recording this will take the interface out of the scren without warning.
        private void ProcessGetSessionsByIdResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.IdleState.ProcessGetSessionsByIdResponse");
            if (args is HttpsResponseArgs)
            {
                Command command = args.Command;

                if (args.ResponseString.Contains("Invalid Session Id"))
                {
                    //depending on where in the polling process the driver is in when the session is deleted
                    //from the website it might be here in the get session by id or in GetRemoteRecorderById so
                    //be ready to catch the deleted session in both places
                    ClearSession();
                }
                else
                {
                    if (!_newRecording)
                    {
                        try
                        {
                            P.NextSession = StateHelper.ProcessNextSessionByRecordingId(args.ResponseString, P.ReportedSessionId, P);
                            P.RecordingConfig.ImportPanoptoSession(P.NextSession);
                            switch (P.NextSession.State)
                            {
                                case SessionState.Scheduled:
                                    {
                                        PanoptoLogger.Notice("SessionState is Scheduled");
                                        switch (args.Command.Name)
                                        {
                                            case CommandName.StartSessionEarlyGetSessionById:
                                                {
                                                    PanoptoLogger.Notice("Command name was {0}. Setting transitioning to true and sending session information", command.Name);
                                                    PanoptoFeedback pf = new PanoptoFeedback()
                                                    {
                                                        FeedbackMessageValue = DisplayMessageEnum.StartingSessionEarly,
                                                        SessionInfo = P.NextSession,
                                                        Transitioning = true
                                                    };
                                                    P.ChangeFeedback(pf);
                                                    break;
                                                }
                                            case CommandName.GetSessionById:
                                                {
                                                    PanoptoLogger.Notice("Command name was {0}. Setting transitioning to false and sending session information", command.Name);
                                                    //This has to be false because if it doesn't the idle screen never reports upcoming sessions
                                                    PanoptoFeedback pf = new PanoptoFeedback()
                                                    {
                                                        FeedbackMessageValue = DisplayMessageEnum.Processing,
                                                        SessionInfo = P.NextSession,
                                                        Transitioning = false
                                                    };
                                                    P.ChangeFeedback(pf);
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                case SessionState.Recording:
                                    {
                                        PanoptoLogger.Notice("SessionState is Recording");
                                        ChangeState(new RecordingState(P), false, DisplayMessageEnum.Recording, P.NextSession);
                                        break;
                                    }
                            }
                            command.Ready = true;
                        }
                        catch (Exception ex)
                        {
                            PanoptoLogger.Error("Panopto.IdleState.ProcessGetSessionByIdResponse Error message is {0}", ex.Message);
                            command.Ignore = true;
                        }
                    }
                    else
                    {
                        PanoptoLogger.Notice("Ignoring response because the user is setting up a new recording");
                        command.Ignore = true;
                    }
                }
                P.SendResult(command);
            }           
        }

        private void ClearSession()
        {
            P.NextSession = new PanoptoSession();
            P.RecordingConfig = new RecordingConfig();
            //sending feedback with no session info provided clears the session info on the idle screen
            PanoptoFeedback pf = new PanoptoFeedback()
            {
                FeedbackMessageValue = DisplayMessageEnum.NoNewSessions,
                Transitioning = false
            };
            P.ChangeFeedback(pf);
        }

        private void SetScheduledRecordingInfoToNow()
        {
            P.RecordingConfig.StartTime = DateTime.Now;
            P.RecordingConfig.EndTime = P.RecordingConfig.StartTime.AddSeconds(P.NextSession.Duration);
            P.RecordingConfig.RecordingId = P.NextSession.RecordingId;
        }

        public override void Stop()
        {
            PanoptoLogger.Notice("Panopto.IdleState.Stop calling _schedulePollingTimer.Reset");
            _schedulePollingTimer.Reset(Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
            _recorderState = RecorderState.Previewing;
            _newRecording = false;
            _early = false;
        }

        public override void Back()
        {
            PanoptoLogger.Notice("Panopto.IdleState.Back calling _schedulePollingTimer.Reset");
            _schedulePollingTimer.Reset(Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
            _recorderState = RecorderState.Previewing;
            _newRecording = false;
            _early = false;
        }

        public override void NewRecording()
        {
            PanoptoLogger.Notice("Panopto.IdleState.NewRecording");
            _schedulePollingTimer.Stop();
            _newRecording = true;
        }

        public override void RecordEarly()
        {
            _early = true;
            PanoptoLogger.Notice("Panopto.IdleState.RecordEarly");
            SetScheduledRecordingInfoToNow();
            P.API.StartSessionEarly(P, CommandType.Control, this);
            PanoptoFeedback pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.SchedulingSession
            };
            P.ChangeFeedback(pf);
            //For a while after the update recording command is sent to the panopto server the remote recorder is still reporting
            //the state previewing even if the command worked and the remote recoder is about to flip to recording. In order to keep
            //from going to the transitional scheduling recording screen back to idle no upcoming sessions and then to now recording
            //instead of the desired scheduled recording to now recording session polling will be paused for a short duration of time.
            //CrestronEnvironment.Sleep(14000);
            _schedulePollingTimer.Reset(Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
        }

        public override void Preview(string recordingName)
        {
            PanoptoLogger.Notice("Panopto.IdleState.Preview");
            PanoptoLogger.Notice("recordingName is {0}", recordingName);
            PanoptoLogger.Notice("Idle: calling ChangeState to switch to preview state");
            SetScheduledRecordingInfoToNow();
            ChangeState(new PreviewState(P), false, DisplayMessageEnum.BeginningPreview);
        }

        private void CheckSchedule(object userspecific)
        {
            PanoptoLogger.Notice("Panopto.IdleState.CheckSchedule");
            PanoptoLogger.Notice("Checking the schedule of the remote recorder");
            if (_early)
            {
                PanoptoLogger.Notice("StartSessionEarlyGetSessionById");
                P.API.StartSessionEarlyGetSessionById(P, CommandType.Poll, this);
            }
            else
            {
                PanoptoLogger.Notice("GetRemoteRecorderById");
                P.API.GetRemoteRecorderById(P, CommandType.Poll, this);
            }
        }
    }
}