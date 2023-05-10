using System;
using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Https;
using Newtonsoft.Json;
using Crestron.SimplSharp.CrestronWebSocketClient;
using Crestron.SimplSharp.CrestronIO;
using System.Collections.Generic;
using CommandType = Crestron.Panopto.Command.CommandType;
using CommandName = Crestron.Panopto.Command.CommandName;

namespace Crestron.Panopto
{
    public class PreviewState : PanoptoState
    {
        private string _audioPreviewUrl;
        private string _videoPreviewUrl;
        private string _thisIp;
        private const string _audioImageFileName = "PanoptoAudioPreviewImage.jpeg";
        private const string _videoImageFileName = "PanoptoVideoPreviewImage.jpeg";
        private string _previewDataTemplate = "{0}/PublicAPI/4.1/RemoteRecorderPreviewData?remoteRecorderId={1}";
        private CTimer _pollingPreviewTimer;
        private CTimer _stateTimer;
        private PanoptoSession _newSession;
        private string _videoRecallAt = string.Empty;
        private string _audioRecallAt = string.Empty;
        private string _videoSaveAt = string.Empty;
        private string _audioSaveAt = string.Empty;
        private readonly CCriticalSection _downloadImageLock = new CCriticalSection();
        private bool _startingRecording = false;

        public PreviewState(Panopto.Driver p)
        {
            PanoptoLogger.Notice("Panopto.PreviewState GUID is {0}", p.RecorderId);
            P = p;
            _thisIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);
            string previewDataUrl = string.Format(_previewDataTemplate, p.PanoptoUrl, p.RecorderId);
            PanoptoLogger.Notice("previewDataUrl is '{0}'", previewDataUrl);
            _pollingPreviewTimer = new CTimer(GetPreviewInfo, null, 0, P.RefreshRate);
        }

        public override string GetStateName()
        {
            return StateConsts.PreviewState;
        }

        public override void SetPreviewRefreshRate(int rate)
        {
            PanoptoLogger.Notice("timerInterval set to {0}", P.RefreshRate);
            if (!_startingRecording)
            {
                _pollingPreviewTimer.Stop();
                _pollingPreviewTimer.Dispose();
                _pollingPreviewTimer = new CTimer(GetPreviewInfo, null, 0, P.RefreshRate);
            }
        }

        public override void StopState()
        {
            PanoptoLogger.Notice("Panopto.PreviewState.StopState");
            if (!_pollingPreviewTimer.Disposed)
            {
                _pollingPreviewTimer.Stop();
                _pollingPreviewTimer.Dispose();
            }
            if (_stateTimer is CTimer && !_stateTimer.Disposed)
            {
                _stateTimer.Stop();
                _stateTimer.Dispose();
            }
        }

        protected void GetPreviewInfo(object notInUse)
        {
            if (!_startingRecording)
            {
                PanoptoLogger.Notice("Panopto.PreviewState.GetPreviewInfo");
                PanoptoLogger.Notice("Getting the urls to download the preview images from");
                P.API.GetPreviewUrls(P, CommandType.Poll, this);
            }
        }

        public override List<PanoptoSession> GetUpcomingRecordings()
        {
            PanoptoLogger.Notice("Panopto.PreviewState.GetUpcomingRecordings");
            PanoptoLogger.Notice("NextSession.Name is {0} StartTime is {1} Duration is {2}", P.NextSession.Name, P.NextSession.StartTime, P.NextSession.Duration);
            List<PanoptoSession> sessions = new List<PanoptoSession>();
            sessions.Add(P.NextSession);
            return sessions;
        }

        private void CheckSchedule(object userspecific)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.CheckSchedule");
            P.API.GetRemoteRecorderById(P, CommandType.Poll, this);
        }

        public override void HttpDownloadDataHandler(HttpsResponseArgs args)
        {
            try
            {
                PanoptoLogger.Notice("Panopto.PreviewState.HttpDownloadDataHandler received response from Panopto");
                Command command = args.Command;
                if (args.ResponseBytes != null && !string.IsNullOrEmpty(args.Url))
                {
                    PanoptoLogger.Notice("the state matches so validating the response");
                    PanoptoFeedback pf = new PanoptoFeedback()
                    {
                        Transitioning = false
                    };
                    if (args.Url.Contains("type=Video"))
                    {
                        _videoRecallAt = string.Format(@"http://{0}/{1}", _thisIp, _videoImageFileName);
                        _videoSaveAt = string.Format(@"/HTML/{0}", _videoImageFileName);
                        P.PreviewData.VideoPreviewUrl = _videoRecallAt;
                        command.Ready = true;
                    }
                    if (args.Url.Contains("type=Audio"))
                    {
                        _audioRecallAt = string.Format(@"http://{0}/{1}", _thisIp, _audioImageFileName);
                        _audioSaveAt = string.Format(@"/HTML/{0}", _audioImageFileName);
                        P.PreviewData.AudioHistogramPreviewUrl = _audioRecallAt;
                        command.Ready = true;
                    }

                    PanoptoLogger.Notice("command.Name is {0}", command.Name);
                    switch (command.Name)
                    {
                        case CommandName.PreviewVideo:
                            {
                                if (!string.IsNullOrEmpty(P.PreviewData.VideoPreviewUrl))
                                {
                                    DownloadImage(args.ResponseBytes, _videoSaveAt, command);
                                    PanoptoLogger.Notice("Panopto.PreviewState.NewVideoPreview");
                                    pf.FeedbackMessageValue = DisplayMessageEnum.NewVideoPreview;
                                    PanoptoLogger.Notice("Sending Preview Video Feedback");
                                    P.ChangeFeedback(pf);
                                }
                                else
                                {
                                    PanoptoLogger.Notice("P.PreviewData.VideoPreviewUrl is null or empty");
                                }
                                break;
                            }
                        case CommandName.PreviewAudio:
                            {
                                if (!string.IsNullOrEmpty(P.PreviewData.AudioHistogramPreviewUrl))
                                {
                                    DownloadImage(args.ResponseBytes, _audioSaveAt, command);
                                    PanoptoLogger.Notice("Panopto.PreviewState.NewAudioPreview");
                                    pf.FeedbackMessageValue = DisplayMessageEnum.NewAudioPreview;
                                    PanoptoLogger.Notice("Sending Preview Audio Feedback");
                                    P.ChangeFeedback(pf);
                                }
                                else
                                {
                                    PanoptoLogger.Notice("P.PreviewData.AudioHistogramPreviewUrl is null or empty");
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.PreviewState.HttpDownloadDataHandler Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
        }

        public override void HttpDataHandler(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.HttpDataHandler received response from Panopto");
            if (!string.IsNullOrEmpty(args.ResponseString))
            {
                PanoptoLogger.Notice(args.ResponseString);
                if (args.ResponseString.Contains("\"Devices\":["))
                {
                    //this is the response to the GetPreviewUrls command
                    //this is a rest command so instead of XML the response is in json
                    //rather then deserialing it into an object the driver will
                    //simply extract the urls directly out of the response string.
                    PanoptoLogger.Notice("Parse Preview Result");
                    ProcessPreviewDataResponse(args);
                }
                else if (args.ResponseString.Contains("<GetRemoteRecordersByIdResponse"))
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
            }
        }

        private void ProcessScheduleRecordingResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.ProcessScheduleRecordingResponse");
            Command command = args.Command;
            command.Ready = true;
            bool result = StateHelper.ProcessScheduleRecordingResponse(args.ResponseString);

            switch (result)
            {
                case true:
                    {
                        PanoptoLogger.Notice("Session scheduled");
                        PanoptoFeedback pf = new PanoptoFeedback()
                        {
                            Transitioning = true,
                            FeedbackMessageValue = DisplayMessageEnum.AbleToScheduleSession
                        };
                        P.ChangeFeedback(pf);
                        _stateTimer = new CTimer(CheckSchedule, null, Panopto.Driver.PollingDueTime, Panopto.Driver.PollingInterval);
                        break;
                    }
                case false:
                    {
                        PanoptoLogger.Notice("Session not scheduled");
                        PanoptoFeedback pf = new PanoptoFeedback()
                        {
                            Transitioning = false,
                            FeedbackMessageValue = DisplayMessageEnum.UnableToScheduleSession
                        };
                        P.ChangeFeedback(pf);
                        break;
                    }
            }
            P.SendResult(command);
        }

        private void ProcessGetDefaultFolderForRecorderResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.ProcessGetDefaultFolderForRecorderResponse");
            _pollingPreviewTimer.Stop();
            Command command = args.Command;
            command.Ready = true;
            P.RecordingConfig.FolderId = new Guid(StateHelper.ProcessGetDefaultFolderForRecorderResponse(args.ResponseString));
            PanoptoLogger.Notice("folderGuid is {0}", P.RecordingConfig.FolderId);
            P.RecordingConfig.EndTime = P.RecordingConfig.StartTime.AddSeconds(P.RecordingConfig.Duration);
            P.API.ScheduleRecording(P, CommandType.Control, this);
            P.SendResult(command);
        }

        private void ProcessPreviewDataResponse(HttpsResponseArgs args)
        {
            try
            {
                PanoptoLogger.Notice("Preview result: {0}", args.ResponseString);
                Command command = args.Command;
                if (!_startingRecording)
                {
                    if (String.IsNullOrEmpty(args.ResponseString) == false)
                    {
                        try
                        {
                            switch (P.Feedback.FeedbackMessageValue)
                            {
                                case DisplayMessageEnum.AbleToScheduleSession:
                                case DisplayMessageEnum.SchedulingSession:
                                    {
                                        PanoptoLogger.Notice("Ignoring preview data while recording session is starting");
                                        _pollingPreviewTimer.Stop();
                                        command.Ignore = true;
                                        break;
                                    }
                                default:
                                    {
                                        PanoptoLogger.Notice("Deserializing remote recorder devices");
                                        RemoteRecorderPreviewData deserializedResult = JsonConvert.DeserializeObject<RemoteRecorderPreviewData>(args.ResponseString);
                                        PreviewData pd = new PreviewData();
                                        if (deserializedResult is RemoteRecorderPreviewData)
                                        {
                                            PanoptoLogger.Notice("data contains devices");
                                            foreach (Device d in deserializedResult.Devices)
                                            {
                                                PanoptoLogger.Notice("processing {0}", d.Name);
                                                if (d.IsPrimaryAudio)
                                                {
                                                    PanoptoLogger.Notice("{0} is primary audio device", d.Name);
                                                    _audioPreviewUrl = d.AudioPreviewUrl;
                                                    P.API.Download(P, _audioPreviewUrl, CommandType.Poll, this);
                                                }
                                                else
                                                {
                                                    PanoptoLogger.Notice("{0} is not primary audio device", d.Name);
                                                }
                                                if (d.IsPrimaryVideo)
                                                {
                                                    PanoptoLogger.Notice("{0} is primary video device", d.Name);
                                                    _videoPreviewUrl = d.VideoPreviewUrl;
                                                    P.API.Download(P, _videoPreviewUrl, CommandType.Poll, this);
                                                }
                                                else
                                                {
                                                    PanoptoLogger.Notice("{0} is not primary video device", d.Name);
                                                }
                                            }
                                            P.PreviewData = pd;
                                            PanoptoLogger.Notice("Panopto.PreviewState.BeginningPreview");
                                            PanoptoFeedback pf2 = new PanoptoFeedback()
                                            {
                                                Transitioning = false,
                                                FeedbackMessageValue = DisplayMessageEnum.BeginningPreview
                                            };
                                            P.ChangeFeedback(pf2);
                                            command.Ready = true;
                                        }
                                        else
                                        {
                                            command.Ignore = true;
                                            PanoptoLogger.Notice("deserializedResult is not a instance of RemoteRecorderPreviewData");
                                        }
                                        break;
                                    }
                            }
                        }
                        catch (Exception)
                        {
                            PanoptoLogger.Error("Unable to parse result {0}", args.ResponseString);
                            command.Ignore = true;
                        }
                    }
                }
                else
                {
                    command.Ignore = true;
                }
                P.SendResult(command);
            }
            catch (Exception ex)
            {
                PanoptoLogger.Error("Panopto.PreviewState.ParseResult Error Message: {0}", ex.Message);
            }
        }

        private void ProcessGetRemoteRecorderByIdResponse(HttpsResponseArgs args)
        {
            Command command = args.Command;
            try
            {
                PanoptoLogger.Notice("Panopto.PreviewState.ProcessGetRemoteRecorderByIdResponse");
                command.Ready = true;
                if (args.ResponseString.Contains("<a:ScheduledRecordings xmlns:b=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"/>"))
                {
                    PanoptoLogger.Notice("No recordings");
                }
                else
                {
                    PanoptoLogger.Notice("Found scheduled recordings");

                    if (P.RecordingConfig == null)
                    {
                        PanoptoLogger.Notice("P.RecordingConfig is null. Initializing P.RecordingConfig");
                        P.RecordingConfig = new RecordingConfig();
                    }

                    PanoptoLogger.Notice("Creating new session");
                    if (P.NextSession == null)
                    {
                        PanoptoLogger.Notice("Panopto.PreviewState.ProcessGetRemoteRecorderByIdResponse P.NextSession is not a instance of PanoptoSession so initializing it");
                        P.NextSession = new PanoptoSession();
                    }

                    P.ReportedSessionId = StateHelper.GetNextSessionGuid(args.ResponseString);

                    P.API.GetSessionById(P, CommandType.Poll, this);
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.PreviewState.ProcessGetRemoteRecorderByIdResponse Error: {0}", e);
                command.Ignore = true;
            }
            P.SendResult(command);
        }

        private void ProcessGetSessionsByIdResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.ProcessGetSessionsByIdResponse");
            Command command = args.Command;
            try
            {
                P.NextSession = StateHelper.ProcessNextSessionByRecordingId(args.ResponseString, P.ReportedSessionId, P);
                if (P.NextSession is PanoptoSession)
                {
                    switch (P.NextSession.State)
                    {
                        case SessionState.Created:
                        case SessionState.Scheduled:
                            {
                                PanoptoLogger.Notice("Preview state, but new session has been created or scheduled");
                                _pollingPreviewTimer.Stop();
                                PanoptoFeedback pf = new PanoptoFeedback()
                                {
                                    SessionInfo = P.NextSession,
                                    Transitioning = true,
                                    FeedbackMessageValue = DisplayMessageEnum.AbleToScheduleSession
                                };
                                P.ChangeFeedback(pf);
                                break;
                            }
                        case SessionState.Broadcasting:
                            {
                                PanoptoLogger.Notice("Broadcasting state");
                                GotoRecordingState();
                                break;
                            }
                        case SessionState.Recording:
                            {
                                PanoptoLogger.Notice("Recording state");
                                GotoRecordingState();
                                break;
                            }
                        default:
                            {
                                PanoptoLogger.Notice("unknown state");
                                break;
                            }
                    }
                }
                command.Ready = true;
            }
            catch (Exception ex)
            {
                PanoptoLogger.Error("Panopto.PreviewState.ProcessGetSessionByIdResponse Error message is {0}", ex.Message);
                command.Ignore = true;
            }
            P.SendResult(command);
        }

        private void ProcessUpdateRecordingTimeResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.ProcessUpdateRecordingTimeResponse commandName.{0}", args.Command.Name);
            Command command = args.Command;
            command.Ready = true;
            if (args.ResponseString.Contains("Fault") || args.ResponseString.Contains("<a:ConflictsExist>true</a:ConflictsExist>"))
            {
                PanoptoLogger.Notice("ResponseString contains fault or conflict");
                PanoptoFeedback pf = new PanoptoFeedback()
                {
                    Transitioning = false,
                    FeedbackMessageValue = DisplayMessageEnum.UnableToScheduleSession
                };
                P.ChangeFeedback(pf);
                if (_newSession is PanoptoSession)
                {
                    PanoptoLogger.Notice("New session name is {0}. It starts at {1} and is {2} long.", _newSession.Name, _newSession.StartTime, _newSession.Duration);
                    P.RescheduledSessionResult(false, _newSession);
                }
            }
            else
            {
                PanoptoLogger.Notice("ResponseString is ack");
                switch (args.Command.Name)
                {
                    case Command.CommandName.RescheduleSession:
                        {
                            PanoptoLogger.Notice("CommandName is RescheduleSession");
                            if (_newSession is PanoptoSession)
                            {
                                PanoptoLogger.Notice("New session name is {0}. It starts at {1} and is {2} long.", _newSession.Name, _newSession.StartTime, _newSession.Duration);
                                P.RescheduledSessionResult(true, _newSession);
                            }
                            break;
                        }
                }
            }
            P.SendResult(command);
        }

        private void GotoRecordingState()
        {
            PanoptoSession sessionInfo = new PanoptoSession()
            {
                Name = P.RecordingConfig.RecordingName,
                Duration = P.RecordingConfig.Duration,
                StartTime = P.RecordingConfig.StartTime,
                IsBroadcast = P.RecordingConfig.IsBroadcast,
                RecordingId = P.NextSession.RecordingId
            };
            P.RecordingConfig.ImportPanoptoSession(sessionInfo);
            ChangeState(new RecordingState(P), false, DisplayMessageEnum.Recording, sessionInfo);
        }

        public override void Record(string recordingName, DateTime startTime, double duration, bool isBroadcast)
        {
            PanoptoLogger.Notice("Processing special characters");
            recordingName = StateHelper.XmlEscape(recordingName);
            if (recordingName.Trim().Length == 0)
            {
                PanoptoLogger.Notice("No recording name so using unnamed as the recording name");
                recordingName = "UnnamedRecording";
            }
            
            PanoptoLogger.Notice("Panopto.PreviewState.Record recordingName is '{0}'", recordingName);
            _pollingPreviewTimer.Stop();
            P.ClearQueue();
            _startingRecording = true;
            P.API.GetDefaultFolderForRemoteRecorder(P, CommandType.Control, this);

            if (P.RecordingConfig is RecordingConfig)
            {
                PanoptoLogger.Notice("RecordingConfig already exists");
            }
            else
            {
                P.RecordingConfig = new RecordingConfig();
            }
            P.RecordingConfig.RecordingName = recordingName;
            P.RecordingConfig.StartTime = startTime;
            P.RecordingConfig.Duration = duration;
            P.RecordingConfig.IsBroadcast = isBroadcast;
            PanoptoLogger.Notice("RecordingConfig: {0}", P.RecordingConfig);
            
            PanoptoFeedback pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.SchedulingSession
            };
            P.ChangeFeedback(pf);
        }

        public override int RecordUntil(string recordingName, bool isBroadcast)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.RecordUntil");
            PanoptoLogger.Notice("recording until nextSession.StartTime = {0}", P.NextSession.StartTime.ToString());
            double timeToRecordInSeconds = P.NextSession.StartTime.Subtract(DateTime.Now).TotalSeconds;
            PanoptoLogger.Notice("timeToRecordInSeconds is {0}", timeToRecordInSeconds);
            Record(recordingName, DateTime.Now, timeToRecordInSeconds, isBroadcast);
            return (int)timeToRecordInSeconds;
        }

        public override void RescheduleSession(Guid sessionId, DateTime startTime, DateTime endTime, PanoptoSession newSession)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.RescheduleSession");
            PanoptoLogger.Notice("sessionId is {0} startTime is {1} endTime is {2}", sessionId, startTime, endTime);
            PanoptoLogger.Notice("New session name is {0}. It starts at {1} and is {2} seconds long.", newSession.Name, newSession.StartTime, newSession.Duration);
            _newSession = newSession;
            P.RecordingConfig.PublicSessionId = sessionId;
            P.RecordingConfig.StartTime = startTime;
            P.RecordingConfig.EndTime = endTime;
            P.API.RescheduleSession(P, CommandType.Control, this);
            PanoptoFeedback pf = new PanoptoFeedback()
            {
                Transitioning = true,
                FeedbackMessageValue = DisplayMessageEnum.SchedulingSession
            };
            P.ChangeFeedback(pf);
        }

        public override void Back()
        {
            ChangeState(new IdleState(P), false, DisplayMessageEnum.SchedulingSession);
        }

        public override void Stop()
        {
            ChangeState(new IdleState(P), false, DisplayMessageEnum.Stopping);
        }

        private void CleanupTimers()
        {
            if (this._pollingPreviewTimer is CTimer)
            {
                this._pollingPreviewTimer.Dispose();
            }
            if (this._stateTimer is CTimer)
            {
                this._stateTimer.Dispose();
            }
        }

        private void DownloadImage(byte[] imageData, string path, Command command)
        {
            PanoptoLogger.Notice("Panopto.PreviewState.DownloadImage command.Name is {0}", command.Name);
            if (imageData != null)
            {
                PanoptoLogger.Notice("imageData.Length is {0}", imageData.Length);
                try
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        try
                        {
                            _downloadImageLock.Enter();
                            PanoptoLogger.Notice("downloading image to {0}", path);
                            try
                            {
                                using (FileStream fileStream = File.Create(path))
                                {
                                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                                    {
                                        writer.Write(imageData, 0, imageData.Length - 1);
                                        PanoptoLogger.Notice("Wrote image to {0}", path);
                                        command.Ready = true;
                                    }
                                }
                            }
                            catch (IOException e)
                            {
                                PanoptoLogger.Error("Panopto.PreviewState.DownloadImage IOException Error");
                                PanoptoLogger.Error(e.ToString());
                                PanoptoLogger.Error("message: {0}", e.Message);
                            }
                        }
                        catch (Exception e)
                        {
                            PanoptoLogger.Error("Panopto.PreviewState.DownloadImage Error");
                            PanoptoLogger.Error(e.ToString());
                            PanoptoLogger.Error("message: {0}", e.Message);
                        }
                        finally
                        {
                            _downloadImageLock.Leave();
                        }
                        
                    }
                    else
                    {
                        PanoptoLogger.Notice("path is empty");
                        command.Ignore = true;
                    }
                }
                catch (Exception e)
                {
                    PanoptoLogger.Error(string.Format("Error extracting image to {0} - Exception: {1}",
                        path, e));
                    command.Ignore = true;
                }
            }
            else
            {
                command.Ignore = true;
            }
            P.SendResult(command);
        }
    }
}