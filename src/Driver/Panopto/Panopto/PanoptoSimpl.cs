using System;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.Panopto;
using System.Collections.Generic;
using System.Globalization;
using Crestron.Panopto.Common.ExtensionMethods;
using Crestron.Panopto.Common.Interfaces;
using Crestron.Panopto.Common;

namespace Crestron.Panopto
{
    //Delegate Definitions

    public delegate void BasicInformationDelegate(
        SimplSharpString description, SimplSharpString guid, SimplSharpString manufacturer,
        SimplSharpString versionDate, ushort supportsFeedback);

    public delegate void PanoptoUpcomingSessionDelegate(
        SimplSharpString sessionName, SimplSharpString sessionMinutesToStart, SimplSharpString sessionSpan,
        SimplSharpString sessionHoursMinutesText, ushort sessionIsBroadcast);

    public delegate void ActiveSessionDelegate(SimplSharpString sessionName, SimplSharpString sessionSpan,
        SimplSharpString sessionReservedForTheNext, SimplSharpString sessionHoursMinutesRemaining,
        ushort sessionEnableExtendBtn, ushort sessionEnablePauseBtn);

    public delegate void PanoptoVideoPreviewDelegate(SimplSharpString videoPreviewUrl);
    public delegate void PanoptoAudioPreviewDelegate(SimplSharpString audioPreviewUrl);

    public delegate void TransitionDelegate(SimplSharpString transitionText);

    public delegate void SessionSubPage(ushort pageNumber);

    public delegate void SessionMainPage(ushort pageNumber);

    public delegate void FeedbackDelegate(SimplSharpString message);

    public delegate void DiagnosticLoggingToggleDelegate(ushort input, SimplSharpString buttonText);

    public class PanoptoSimpl : IObserver
    {
        //delegates
        public BasicInformationDelegate BasicInformationUpdated { get; set; }
        public PanoptoUpcomingSessionDelegate PanoptoUpcomingSessionUpdated { get; set; }
        public ActiveSessionDelegate PanoptoActiveSessionUpdated { get; set; }
        public PanoptoVideoPreviewDelegate PanoptoVideoPreviewUpdated { get; set; }
        public PanoptoAudioPreviewDelegate PanoptoAudioPreviewUpdated { get; set; }
        public TransitionDelegate PanoptoTransitionSessionUpdated { get; set; }
        public SessionSubPage PanoptoSessionSubPageUpdated { get; set; }
        public SessionMainPage PanoptoMainPageUpdated { get; set; }
        public FeedbackDelegate PanoptoFeedbackMessageUpdated { get; set; }
        public DiagnosticLoggingToggleDelegate LoggingToggled { get; set; }
        public CurrentPage CurrentIdlePage;
        public CurrentPage CurrentPage;
        public Driver MyPanopto;
        private string _panoptoState = string.Empty;
        private Dictionary<DisplayMessageEnum, string> _feedbackMessages;
        private bool _broadcast;
        private string _recordingName;
        private double _duration;
        private const string _LoggingEnabled = "Logging Enabled";
        private string _previousVideoUrl = string.Empty;
        private string _previousAudioUrl = string.Empty;

        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        public PanoptoSimpl()
        {
        }

        private void PanoptoConstructorTimerCallback(object NotUsed)
        {
            Initialize();
        }

        public void Initialize()
        {
            try
            {
                PanoptoLogger.Notice("PanoptoSimpl.Initialize");
                if (BasicInformationUpdated != null)
                {
                    BasicInformationUpdated(new SimplSharpString(MyPanopto.Description),
                        new SimplSharpString(MyPanopto.Guid.ToString()),
                        new SimplSharpString(MyPanopto.Manufacturer),
                        new SimplSharpString(MyPanopto.VersionDate.ToString(CultureInfo.InvariantCulture)),
                        MyPanopto.SupportsFeedback ? (ushort)1 : (ushort)0);
                }

                if (PanoptoUpcomingSessionUpdated != null)
                {
                    PanoptoUpcomingSessionUpdated(new SimplSharpString("Unknown"), new SimplSharpString("Unknown"),
                        new SimplSharpString("Unknown"), new SimplSharpString("Unknown"), 0);
                }

                if (PanoptoActiveSessionUpdated != null)
                {
                    PanoptoActiveSessionUpdated(new SimplSharpString(""), new SimplSharpString(""),
                        new SimplSharpString(""), new SimplSharpString(""), 0, 0);
                }

                if (PanoptoMainPageUpdated != null)
                {
                    PanoptoMainPageUpdated((ushort)CurrentPage.InitializationPage);
                }

                if (PanoptoSessionSubPageUpdated != null)
                {
                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableAllDay);
                }

                if (PanoptoFeedbackMessageUpdated != null)
                {
                    PanoptoFeedbackMessageUpdated(new SimplSharpString("Unknown"));
                }
            }
            catch
            {
            }
        }

        private void PopulateFeedbackMessageDictionary()
        {
            _feedbackMessages = new Dictionary<DisplayMessageEnum, string>();
            _feedbackMessages.Add(DisplayMessageEnum.AbleToAccessRemoteRecorder, "Successfully accessed remote recorder");
            _feedbackMessages.Add(DisplayMessageEnum.AbleToScheduleSession, "Successfully scheduled session");
            _feedbackMessages.Add(DisplayMessageEnum.AccessingRemoteRecorder, "Accessing remote recorder");
            _feedbackMessages.Add(DisplayMessageEnum.BeginningPause, "Pausing...");
            _feedbackMessages.Add(DisplayMessageEnum.BeginningPreview, "Getting Preview...");
            _feedbackMessages.Add(DisplayMessageEnum.ExtendingSession, "Extending session...");
            _feedbackMessages.Add(DisplayMessageEnum.LoggingIn, "Logging in...");
            _feedbackMessages.Add(DisplayMessageEnum.NewAudioPreview, "New audio preview found");
            _feedbackMessages.Add(DisplayMessageEnum.NewVideoPreview, "New video preview found");
            _feedbackMessages.Add(DisplayMessageEnum.NoNewSessions, "No new Sessions");
            _feedbackMessages.Add(DisplayMessageEnum.Recording, "Recording session");
            _feedbackMessages.Add(DisplayMessageEnum.Resuming, "Resuming...");
            _feedbackMessages.Add(DisplayMessageEnum.SchedulingSession, "Scheduling session...");
            _feedbackMessages.Add(DisplayMessageEnum.StartingSessionEarly, "Starting session early...");
            _feedbackMessages.Add(DisplayMessageEnum.Stopping, "Stopping session...");
            _feedbackMessages.Add(DisplayMessageEnum.UnableToAccessRemoteRecorder, "Unable to access remote recorder");
            _feedbackMessages.Add(DisplayMessageEnum.UnableToExtendSession, "Unable to extend session");
            _feedbackMessages.Add(DisplayMessageEnum.UnableToLogIn, "Unable to Log In");
            _feedbackMessages.Add(DisplayMessageEnum.UnableToResume, "Unable to resume session");
            _feedbackMessages.Add(DisplayMessageEnum.UnableToScheduleSession, "Unable to schedule session");
            _feedbackMessages.Add(DisplayMessageEnum.UnableToStartSessionEarly, "Unable to start session early");
            _feedbackMessages.Add(DisplayMessageEnum.UnableToStop, "Unable to stop session");
            _feedbackMessages.Add(DisplayMessageEnum.Processing, "Processing recording");
            _feedbackMessages.Add(DisplayMessageEnum.Uploading, "Uploading recording");
            _feedbackMessages.Add(DisplayMessageEnum.InvalidRecordingName, "Invalid recording name");
            //this message string is blank on purpose. It's my way of clearing the message on screen
            _feedbackMessages.Add(DisplayMessageEnum.Unknown, "");
        }

        /// <summary>
        /// Create Panopto should only be called after the UI comes online
        /// </summary>
        private void CreatePanopto()
        {
            try
            {
                PanoptoLogger.Notice("PanoptoSimpl.CreatePanopto");
                if (MyPanopto is Panopto.Driver)
                {
                    PanoptoLogger.Notice("Reusing existing Panopto object by setting it to InitialState");
                    MyPanopto.ChangeState(new InitialState(MyPanopto, new CrestronDataStoreWrapper()), false, DisplayMessageEnum.LoggingIn, null);
                }
                else
                {
                    CurrentPage = CurrentPage.InitializationPage;
                    _panoptoState = StateConsts.InitialState;
                    PopulateFeedbackMessageDictionary();
                    MyPanopto = new Panopto.Driver();
                    if (MyPanopto is Panopto.Driver)
                    {
                        PanoptoLogger.Notice("MyPanopto is Panopto.Driver");
                        Initialize();

                        MyPanopto.PreviewData = new PreviewData();
                        MyPanopto.PreviewData.AudioHistogramPreviewUrl = String.Empty;
                        MyPanopto.PreviewData.VideoPreviewUrl = String.Empty;

                        MyPanopto.Register(this);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception(e.StackTrace, e);
            }
        }

        public void EnableLoggingBtn()
        {
            MyPanopto.DiagnosticLoggingEnabled = !MyPanopto.DiagnosticLoggingEnabled;
            var loggingEnabled = MyPanopto.DiagnosticLoggingEnabled;
            LoggingToggled(Convert.ToUInt16(loggingEnabled), loggingEnabled ? _LoggingEnabled : string.Empty);
        }

        public void Log(SimplSharpString message)
        {
            PanoptoLogger.Notice(message.ToString());
        }

        private void ConnectionChanged(ConnectionFeedback feedback)
        {
            PanoptoLogger.Notice("PanoptoSimpl.ConnectionChanged");
            PanoptoLogger.Notice("feedback.State is '{0}'", feedback.State);
            switch (feedback.State)
            {
                case true:
                    {
                        if (!feedback.Transitioning)
                        {
                            PanoptoSessionSubPageUpdated((ushort)CurrentPage);
                        }
                        break;
                    }
                case false:
                    {
                        PanoptoSessionSubPageUpdated((ushort)CurrentPage.Offline);
                        CurrentPage = CurrentPage.Offline;
                        break;
                    }
            }
        }

        private void FeedbackChanged(PanoptoFeedback feedback)
        {
            PanoptoLogger.Notice("PanoptoSimpl.FeedbackChanged feedback.FeedbackMessageValue is '{0}' and feedback.Transitioning is '{1}'", feedback.FeedbackMessageValue, feedback.Transitioning);

            if (feedback.ServerDisconnect)
            {
                PanoptoLogger.Notice("Panopto.PanoptoSimpl.FeedbackChanged feedback.ServerDisconnect is true");
                PanoptoSessionSubPageUpdated((ushort)CurrentPage.Offline);
                CurrentPage = CurrentPage.Offline;
            }
            else if (feedback.Transitioning != null)
            {
                if ((bool)feedback.Transitioning)
                {
                    PanoptoLogger.Notice("Feedback is transitioning feedback.FeedbackMessageValue is {0}", feedback.FeedbackMessageValue);
                    PanoptoTransitionSessionUpdated(_feedbackMessages[feedback.FeedbackMessageValue]);
                    if (CurrentPage == CurrentPage.InitializationPage)
                    {
                        PanoptoMainPageUpdated((ushort)CurrentPage.MainPage);
                    }
                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.RecorderLoading);
                    CurrentPage = CurrentPage.RecorderLoading;
                }
                else
                {
                    switch (_panoptoState)
                    {
                        case StateConsts.InitialState:
                            {
                                PanoptoLogger.Notice("Panopto State is InitialState");
                                PanoptoMainPageUpdated((ushort)CurrentPage.InitializationPage);
                                CurrentPage = CurrentPage.InitializationPage;
                                break;
                            }
                        case StateConsts.IdleState:
                            {
                                PanoptoLogger.Notice("Feedback changed while in Idle state");
                                bool availableAllDay = true;
                                if (feedback.FeedbackMessageValue == DisplayMessageEnum.SchedulingSession)
                                {
                                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.RecordNow);
                                }
                                else if (feedback.FeedbackMessageValue == DisplayMessageEnum.NoNewSessions)
                                {
                                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableAllDay);
                                    CurrentPage = CurrentPage.AvailableAllDay;
                                    CurrentIdlePage = CurrentPage.AvailableAllDay;
                                }
                                else
                                {
                                    if (feedback.SessionInfo.Exists())
                                    {
                                        PanoptoLogger.Notice("SessionInfo exists, assuming we need to show upcoming meetings info");
                                        if (feedback.SessionInfo.StartTime.Date == DateTime.Today)
                                        {
                                            var remainingTime = feedback.SessionInfo.StartTime - DateTime.Now;
                                            if (remainingTime.Minutes > 0)
                                            {
                                                ProcessUpcomingSession(feedback.SessionInfo);
                                                PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableUpcomingMeeting);
                                                CurrentPage = CurrentPage.AvailableUpcomingMeeting;
                                                CurrentIdlePage = CurrentPage.AvailableUpcomingMeeting;
                                                availableAllDay = false;
                                            }
                                        }
                                        else
                                        {
                                            //the scheduled session is not for today so it is available all day
                                            availableAllDay = true;
                                        }
                                    }
                                    else
                                    {
                                        //no session means it is available all day
                                        availableAllDay = true;
                                    }

                                    if (availableAllDay)
                                    {
                                        PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableAllDay);
                                        CurrentPage = CurrentPage.AvailableAllDay;
                                        CurrentIdlePage = CurrentPage.AvailableAllDay;
                                    }
                                }
                                break;
                            }
                        case StateConsts.RecordingState:
                        case StateConsts.BroadcastState:
                            {
                                PanoptoLogger.Notice("Feedback changed while in Recording or Broadcast state");
                                if (feedback.SessionInfo.Exists())
                                {
                                    ProcessActiveSession(feedback.SessionInfo);
                                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.NowRecording);
                                    CurrentPage = CurrentPage.NowRecording;
                                }
                                break;
                            }
                        case StateConsts.PauseState:
                            {
                                PanoptoLogger.Notice("Feedback changed while in Pause state");
                                if (feedback.SessionInfo.Exists())
                                {
                                    ProcessActiveSession(feedback.SessionInfo);
                                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.PausedRecording);
                                    CurrentPage = CurrentPage.PausedRecording;
                                }
                                break;
                            }
                        case StateConsts.PreviewState:
                            {
                                PanoptoLogger.Notice("Feedback changed while in Preview state");
                                if (feedback.FeedbackMessageValue == DisplayMessageEnum.UnableToScheduleSession)
                                {
                                    PanoptoLogger.Notice("UnableToScheduleSession");
                                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.ConflictingSessions);
                                    CurrentPage = CurrentPage.ConflictingSessions;
                                }
                                else if (CurrentPage != CurrentPage.ConflictingSessions)
                                {
                                    PanoptoLogger.Notice("Current page is PreviewSelection");
                                    PanoptoMainPageUpdated((ushort)CurrentPage.MainPage);
                                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.PreviewSelection);
                                    SendPreviewUrls();
                                    CurrentPage = CurrentPage.PreviewSelection;
                                }
                                break;
                            }
                    }
                }
            }

            if (feedback.FeedbackMessageValue.Exists())
            {
                PanoptoFeedbackMessageUpdated(new SimplSharpString(_feedbackMessages[feedback.FeedbackMessageValue]));
            }
        }

        private void StateChanged(PanoptoFeedback pf)
        {
            string state = pf.DriverState;
            PanoptoLogger.Notice("PanoptoSimpl.StateChanged state is {0}", state);
            string previousState = _panoptoState;
            PanoptoLogger.Notice("Previous State is {0}", previousState);
            _panoptoState = state;

            try
            {
                if (previousState.Equals(StateConsts.PreviewState) && _panoptoState.Equals(StateConsts.IdleState))
                {
                    PanoptoLogger.Notice("Previous state is Preview State and Current State is Idle");
                    switch (CurrentIdlePage)
                    {
                        case CurrentPage.AvailableUpcomingMeeting:
                            PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableUpcomingMeeting);
                            CurrentPage = CurrentPage.AvailableUpcomingMeeting;
                            CurrentIdlePage = CurrentPage.AvailableUpcomingMeeting;
                            break;
                        case CurrentPage.AvailableAllDay:
                            PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableAllDay);
                            CurrentPage = CurrentPage.AvailableAllDay;
                            CurrentIdlePage = CurrentPage.AvailableAllDay;
                            break;
                    }
                }
                else
                {
                    switch (state)
                    {
                        case StateConsts.IdleState:
                            {
                                PanoptoMainPageUpdated((ushort)CurrentPage.MainPage);
                                PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableAllDay);
                                CurrentPage = CurrentPage.AvailableAllDay;
                                CurrentIdlePage = CurrentPage.AvailableAllDay;
                                break;
                            }
                        case StateConsts.InitialState:
                            {
                                PanoptoFeedbackMessageUpdated(new SimplSharpString(string.Empty));
                                PanoptoMainPageUpdated((ushort)CurrentPage.InitializationPage);
                                CurrentPage = CurrentPage.InitializationPage;
                                break;
                            }
                        case StateConsts.PauseState:
                            {
                                PanoptoMainPageUpdated((ushort)CurrentPage.MainPage);
                                PanoptoSessionSubPageUpdated((ushort)CurrentPage.PausedRecording);
                                CurrentPage = CurrentPage.PausedRecording;
                                break;
                            }
                        case StateConsts.RecordingState:
                        case StateConsts.BroadcastState:
                            {
                                PanoptoLogger.Notice("Feedback changed while in Recording or Broadcast state");
                                if (pf.SessionInfo.Exists())
                                {
                                    PanoptoSessionSubPageUpdated((ushort)CurrentPage.NowRecording);
                                    CurrentPage = CurrentPage.NowRecording;
                                }
                                else
                                {
                                    PanoptoLogger.Notice("Session info doesn't exist");
                                }
                                break;
                            }
                        case StateConsts.PreviewState:
                            {
                                PanoptoMainPageUpdated((ushort)CurrentPage.MainPage);
                                PanoptoSessionSubPageUpdated((ushort)CurrentPage.PreviewSelection);
                                CurrentPage = CurrentPage.PreviewSelection;
                                try
                                {
                                    SendPreviewUrls();
                                }
                                catch (Exception e)
                                {
                                    PanoptoLogger.Error(e.Message);
                                    PanoptoLogger.Error(e.StackTrace);
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error(e.Message);
                PanoptoLogger.Error(e.StackTrace);
            }
        }

        private void SendPreviewUrls()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.SendPreviewUrls");
            if (MyPanopto.PreviewData is PreviewData)
            {
                if (MyPanopto.PreviewData.VideoPreviewUrl is string && !MyPanopto.PreviewData.VideoPreviewUrl.Equals(_previousVideoUrl))
                {
                    PanoptoLogger.Notice("Video URL is new string");
                    if (!string.IsNullOrEmpty(MyPanopto.PreviewData.VideoPreviewUrl))
                    {
                        PanoptoLogger.Notice("calling PanoptoVideoPreviewUpdated Video URL is {0}", MyPanopto.PreviewData.VideoPreviewUrl);
                        PanoptoVideoPreviewUpdated(MyPanopto.PreviewData.VideoPreviewUrl);
                        _previousVideoUrl = MyPanopto.PreviewData.VideoPreviewUrl;
                    }
                    else
                    {
                        PanoptoLogger.Notice("Video preview url is empty");
                    }
                }
                else
                {
                    PanoptoLogger.Notice("MyPanopto.PreviewData.VideoPreviewUrl is null");
                }
                if (MyPanopto.PreviewData.AudioHistogramPreviewUrl is string && !MyPanopto.PreviewData.AudioHistogramPreviewUrl.Equals(_previousAudioUrl))
                {
                    PanoptoLogger.Notice("Audio URL is new string");
                    if (!string.IsNullOrEmpty(MyPanopto.PreviewData.AudioHistogramPreviewUrl))
                    {
                        PanoptoLogger.Notice("calling PanoptoAudioPreviewUpdated Audio URL is {0}", MyPanopto.PreviewData.AudioHistogramPreviewUrl);
                        PanoptoAudioPreviewUpdated(MyPanopto.PreviewData.AudioHistogramPreviewUrl);
                        _previousAudioUrl = MyPanopto.PreviewData.AudioHistogramPreviewUrl;
                    }
                    else
                    {
                        PanoptoLogger.Notice("Audio preview url is empty");
                    }
                }
                else
                {
                   PanoptoLogger.Notice("MyPanopto.PreviewData.AudioHistogramPreviewUrl is null");
                }
            }
            else
            {
                PanoptoLogger.Notice("!MyPanopto.PreviewData is PreviewData");
            }
        }

        private void ProcessUpcomingSession(PanoptoSession session)
        {
            var remainingTime = session.StartTime - DateTime.Now;
            string availableForTheNextText;
            string hoursMinutesText;

            string spanText = "From " + session.StartTime.ToShortTimeString() + " to " +
                              session.StartTime.AddSeconds(session.Duration).ToShortTimeString();

            if (remainingTime.Minutes < 0)
            {
                availableForTheNextText = "--";
                hoursMinutesText = "MINUTES";
                PanoptoUpcomingSessionUpdated(session.Name, availableForTheNextText, spanText, hoursMinutesText,
                    (ushort)Convert.ToInt16(session.IsBroadcast));
            }
            else if (remainingTime.Hours == 0 && remainingTime.Minutes < 60)
            {
                availableForTheNextText = remainingTime.Minutes.ToString("00");
                hoursMinutesText = "MINUTES";
                PanoptoUpcomingSessionUpdated(session.Name, availableForTheNextText, spanText, hoursMinutesText,
                    (ushort)Convert.ToInt16(session.IsBroadcast));
            }
            else if (remainingTime.Hours > 0)
            {
                availableForTheNextText = remainingTime.Hours.ToString("####") + ":" + remainingTime.Minutes.ToString("00");
                hoursMinutesText = "HOURS\\MINUTES";
                PanoptoUpcomingSessionUpdated(session.Name, availableForTheNextText, spanText, hoursMinutesText,
                    (ushort)Convert.ToInt16(session.IsBroadcast));
            }
        }

        private void ProcessActiveSession(PanoptoSession session)
        {
            TimeSpan remainingTime = session.StartTime.AddSeconds(session.Duration) - DateTime.Now;
            string remainingTimeText;
            string remainingHoursMinutesText;

            string spanText = "From " + session.StartTime.ToShortTimeString() + " to " +
                              session.StartTime.AddSeconds(session.Duration).ToShortTimeString();


            if (remainingTime.Minutes < 0)
            {
                remainingTimeText = "--";
                remainingHoursMinutesText = "MINUTES";
                PanoptoActiveSessionUpdated(session.Name, spanText, remainingTimeText, remainingHoursMinutesText, 1,
                    (ushort)Convert.ToInt16(!session.IsBroadcast));
            }
            else if (remainingTime.Hours == 0 && remainingTime.Minutes <= 5 && remainingTime.Minutes != 0)
            {
                remainingTimeText = remainingTime.Minutes.ToString("00");
                remainingHoursMinutesText = "MINUTES";
                PanoptoActiveSessionUpdated(session.Name, spanText, remainingTimeText, remainingHoursMinutesText, 1,
                    (ushort)Convert.ToInt16(!session.IsBroadcast));
            }
            else if (remainingTime.Hours == 0 && remainingTime.Minutes < 60 && remainingTime.Minutes > 5)
            {
                remainingTimeText = remainingTime.Minutes.ToString("00");
                remainingHoursMinutesText = "MINUTES";
                PanoptoActiveSessionUpdated(session.Name, spanText, remainingTimeText, remainingHoursMinutesText, 0,
                        (ushort)Convert.ToInt16(!session.IsBroadcast));
            }
            else if (remainingTime.Hours > 0)
            {
                remainingTimeText = remainingTime.Hours.ToString("####") + ":" + remainingTime.Minutes.ToString("00");
                remainingHoursMinutesText = "HOURS\\MINUTES";
                PanoptoActiveSessionUpdated(session.Name, spanText, remainingTimeText, remainingHoursMinutesText, 0,
                    (ushort)Convert.ToInt16(!session.IsBroadcast));
            }
        }

        public ushort PanoptoLogin(SimplSharpString panoptoUrl, SimplSharpString userName,
            SimplSharpString userPassword, SimplSharpString recorderName, SimplSharpString ipAddress)
        {
            try
            {
                //this will prevent attempting to login if any of the fields on the login are empty except for the ip address which is optional
                if (!string.IsNullOrEmpty(panoptoUrl.ToString().Trim()) && !string.IsNullOrEmpty(userName.ToString().Trim()) && !string.IsNullOrEmpty(userPassword.ToString().Trim()) && !string.IsNullOrEmpty(recorderName.ToString().Trim()))
                {
                    string ConnectionURL = NormalizeUrl(panoptoUrl.ToString());
                    string escapedRemoteRecorderName = StateHelper.XmlEscape(recorderName.ToString());
                    //this is here for now until Panopto responds with how to handle special characters for the password parameter
                    //string escapedPassword = StateHelper.XmlEscape(userPassword.ToString());
                    //MyPanopto.Configure(ConnectionURL, userName.ToString(), escapedPassword, escapedRemoteRecorderName, ipAddress.ToString(), string.Empty, true, DisplayMessageEnum.LoggingIn);
                    MyPanopto.Configure(ConnectionURL, userName.ToString(), userPassword.ToString(), escapedRemoteRecorderName, ipAddress.ToString(), string.Empty, true, DisplayMessageEnum.LoggingIn);
                }
                else
                {
                    PanoptoLogger.Notice("Logging attempt aborted because required fields are empty");
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Notice("Issue with login attempt: {0}", e.Message);
                PanoptoLogger.Notice("Stack: {0}", e.StackTrace);
            }
            return 0;
        }

        public string NormalizeUrl(String text)
        {
            string NewUrl = string.Empty;
            if (text != String.Empty)
            {
                if (text.StartsWith("http://"))
                {
                    text = text.Replace("http://", "https://");
                }
                else if (text.StartsWith("https://") == false)
                {
                    text = "https://" + text;
                }

                string temp = text.ToLower();
                if (temp.EndsWith("/panopto"))
                {
                    //removes any instance of panopto regardless of case
                    text = text.Substring(0, text.Length - 8);
                }
                //adds in Panopto exactly like this
                text += "/Panopto";
                //panopto driver url field = text
            }
            NewUrl = text;
            return NewUrl;
        }

        /// <summary>
        /// When the UI signals that it is online, call the CreatePanopto method
        /// </summary>
        public void PanelOnline()
        {
            CreatePanopto();
        }

        public void RecordEarlyBtn()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.RecordEarlyBtn");
            MyPanopto.StartRecordingEarly();
        }

        public void RecordingStopBtn()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.RecordingStopBtn");
            MyPanopto.StopRecording();
        }

        public void PreviewBtn(SimplSharpString recordingName)
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.PreviewBtn");
            MyPanopto.Preview(recordingName.ToString());
        }

        public void PreviewEarlyBtn(SimplSharpString recordingName)
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.PreviewEarlyBtn");
            MyPanopto.Preview(recordingName.ToString());
        }

        public void RecordingPauseBtn()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.RecordingPauseBtn");
            MyPanopto.Pause();
        }

        public void RecordingResumeBtn()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.RecordingResumeBtn");
            MyPanopto.Resume();
        }

        public void RecordingExtendBtn()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.RecordingExtendBtn");
            MyPanopto.ExtendRecording();
        }

        public void BackBtn()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.BackBtn CurrentPage is {0}", CurrentPage);
            MyPanopto.Back();
            switch (CurrentIdlePage)
            {
                case CurrentPage.AvailableUpcomingMeeting:
                    {
                        PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableUpcomingMeeting);
                        break;
                    }
                case CurrentPage.AvailableAllDay:
                    {
                        PanoptoSessionSubPageUpdated((ushort)CurrentPage.AvailableAllDay);
                        break;
                    }
            }
        }

        public void CancelRecordingBtn()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.CancelRecordingBtn");
            BackBtn();
        }

        public void NewRecordingBtn()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.NewRecordingBtn");
            MyPanopto.NewRecording();
            PanoptoSessionSubPageUpdated((ushort)CurrentPage.RecordNow);
        }

        public void RecordBtn(SimplSharpString recordingTitle, ushort recordingDuration, ushort recordingBroadCast)
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.RecordBtn");
            _duration = Convert.ToDouble(recordingDuration);
            _recordingName = recordingTitle.ToString();
            PanoptoLogger.Notice("{0} duration is {1}", _recordingName, _duration);

            if (recordingBroadCast == 1)
            {
                _broadcast = true;
            }
            else
            {
                _broadcast = false;
            }

            MyPanopto.RecordNow(_recordingName, _broadcast, _duration);
        }

        public void RecordUntil(SimplSharpString recordingName, ushort isBroadcast)
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.RecordUntil");

             _recordingName = recordingName.ToString();

            if (isBroadcast == 1)
            {
                _broadcast = true;
            }
            else
            {
                _broadcast = false;
            }

            _duration = MyPanopto.RecordUntil(_recordingName, _broadcast);
        }

        public void RescheduleConflict(SimplSharpString recordingTitle, ushort recordingDuration, ushort recordingBroadCast)
        {
            try
            {
                PanoptoLogger.Notice("Panopto.PanoptoSimpl.RescheduleConflict");

                List<PanoptoSession> upcomingSessions = MyPanopto.GetUpcomingSessions();
                PanoptoSession nextSession = upcomingSessions[0];
                PanoptoLogger.Notice("nextSession.Duration is {0}", nextSession.Duration);
                PanoptoLogger.Notice("recordingDuration is {0}", recordingDuration);

                //the new session that is requested to come before the conflicting session
                DateTime newSessionStart = DateTime.Now;
                DateTime newSessionEnd = newSessionStart.AddSeconds((double)recordingDuration);
                PanoptoSession newSession = new PanoptoSession()
                {
                    StartTime = newSessionStart,
                    Duration = recordingDuration
                };

                //the orginal session being pushed back to start 1 minute after the new session completes and record for the orginal length of time
                DateTime originalSessionNewStart = newSessionEnd.AddMinutes(1);
                DateTime originalSessionNewEnd = originalSessionNewStart.AddSeconds(nextSession.Duration);

                PanoptoLogger.Notice("newSessionStart is {0} newSessionEnd is {1} newSessionDuration is {2} ", newSessionStart, newSessionEnd, recordingDuration);
                PanoptoLogger.Notice("originalSessionNewStart is {0} originalSessionNewEnd is {1} originalSessionSpan is {2}", originalSessionNewStart, originalSessionNewEnd, nextSession.Duration);

                //issue the command to reschedule the existing session and start the process of creating the new recording session before it
                MyPanopto.RescheduleRecording(nextSession.RecordingId, originalSessionNewStart, originalSessionNewEnd, newSession);
            }
            catch (Exception ex)
            {
                PanoptoLogger.Error("Error at Panopto.PanoptoSimpl.RescheduleConflict Message: {0}", ex.Message);
            }
        }

        public void RescheduleConflictResult(bool result, PanoptoSession newSession)
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.RescheduleConflictResult result is {0}", result);
            PanoptoLogger.Notice("Session name is {0}. It starts at {1} and is {2} long.", newSession.Name, newSession.StartTime, newSession.Duration);
            if (result)
            {
                this.RecordBtn(new SimplSharpString(_recordingName), (ushort)newSession.Duration, Convert.ToUInt16(_broadcast));
            }
        }

        public void SetPreviewRefreshRate(uint rate)
        {
            if (MyPanopto == null)
            {
                CrestronConsole.PrintLine("Panopto is null!");
            }
            try
            {
                MyPanopto.SetPreviewRefreshRate(Convert.ToInt32(rate));
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("Error Calling MyPanopto.SetPreviewRefreshRate message is '{0]'", ex.Message);
            }
        }

        public void Reconfigure()
        {
            PanoptoLogger.Notice("Panopto.PanoptoSimpl.Reconfigure");
            MyPanopto.Reconfigure();
        }

        #region IObserver Members

        public void Update(object updateInfo)
        {
            PanoptoLogger.Notice("PanoptoSimpl.Update");
            if (updateInfo is PanoptoFeedback)
            {
                PanoptoLogger.Notice("updateInfo is PanoptoFeedback");
                PanoptoFeedback pf = updateInfo as PanoptoFeedback;
                if (pf.DriverState.Exists())
                {
                    PanoptoLogger.Notice("State Changed");
                    StateChanged(pf);
                }
                else
                {
                    PanoptoLogger.Notice("Feedback Changed");
                    FeedbackChanged(pf);
                }
            }
            else if (updateInfo is ConnectionFeedback)
            {
                PanoptoLogger.Notice("updateInfo is ConnectionFeedback");
                ConnectionFeedback cf = updateInfo as ConnectionFeedback;
            }
            else if (updateInfo is CurrentPage)
            {
                PanoptoLogger.Notice("updateInfo is int which is assumed to be the page value");
                PanoptoSessionSubPageUpdated((ushort)updateInfo);
            }
        }

        #endregion
    }
}
