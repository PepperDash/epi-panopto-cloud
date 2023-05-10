// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.Text;
using Crestron.Panopto.Common.Interfaces;

namespace Crestron.Panopto.Common.Interfaces
{
    public enum DisplayMessageEnum
    {
        Unknown,
        LoggingIn,
        AbleToAccessRemoteRecorder,
        AccessingRemoteRecorder,
        SchedulingSession,
        UnableToScheduleSession,
        AbleToScheduleSession,
        StartingSessionEarly,
        UnableToStartSessionEarly,
        ExtendingSession,
        UnableToExtendSession,
        BeginningPreview,
        BeginningPause,
        Recording,
        Resuming,
        Stopping,
        UnableToStop,
        UnableToAccessRemoteRecorder,
        UnableToLogIn,
        NoNewSessions,
        UnableToResume,
        NewVideoPreview,
        NewAudioPreview,
        Processing,
        Uploading,
        InvalidRecordingName
    }
    public static class StateConsts
    {
        public const string InitialState = "Initial";
        public const string IdleState = "Idle";
        public const string PreviewState = "Previewing";
        public const string BroadcastState = "Broadcasting";
        public const string RecordingState = "Recording";
        public const string PauseState = "Paused";
    }

    public enum RecorderState
    {
        Unknown = 0,
        Previewing = 1,
        Scheduled = 2,
        Recording = 3,
        Uploading = 4,
        Processing = 5,
        Complete = 6,
        Stopped = 7
    }

    public enum SessionState
    {
        Unknown = 0,
        Created = 1,
        Scheduled = 2,
        Recording = 3,
        Broadcasting = 4,
        Processing = 5,
        Uploading = 6,
        Complete = 7
    }

    public enum CurrentPage
    {
        Unknown = 0,
        AvailableAllDay = 1,
        AvailableUpcomingMeeting = 2,
        NowRecording = 3,
        PreviewSelection = 4,
        PausedRecording = 5,
        RecordNow = 6,
        RecorderLoading = 8,
        PreviewEarly = 9,
        MainPage = 10,
        InitializationPage = 11,
        Offline = 12,
        ConflictingSessions = 13
    }

    public class PanoptoSession
    {
        public Guid RecordingId { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public SessionState State { get; set; }
        public double Duration { get; set; }
        public bool IsBroadcast { get; set; }
        public string Name { get; set; }
        public bool Processed { get; set; }
    }

    public class ConnectionFeedback
    {
        public bool State { get; set; }
        public bool Transitioning { get; set; }
    }

    public class PanoptoFeedback
    {
        public string DriverState { get; set; }
        public bool? Transitioning { get; set; }
        public bool ServerDisconnect { get; set; }
        public CurrentPage SetPageTo { get; set; }
        public CurrentPage SetSubpageTo { get; set; }
        public DisplayMessageEnum FeedbackMessageValue { get; set; }
        public PanoptoSession SessionInfo { get; set; }
    }

    public class PreviewData
    {
        public string AudioHistogramPreviewUrl { get; set; }
        public string VideoPreviewUrl { get; set; }
    }
    /// <summary>
    /// Interface that exposes methods for interacting with a Panopto server API.
    /// </summary>
    public interface IPanopto : IBasicInformation, IDisposable, ISubject
    {
        double StandardExtensionTime { get; set; }
        PreviewData PreviewData { get; set; }
        bool DiagnosticLoggingEnabled { get; set; }

        bool Configure(string panoptoUrl, string userName, string password, string recorderName, string ipAddress);

        void SetPreviewRefreshRate(int rate);

        bool ScheduleRecording(string recordingName, bool isBroadcast, DateTime startTime, double duration);

        bool RescheduleRecording(string sessionId, DateTime startTime, DateTime endTime);

        bool RecordNow(string recordingName, bool isBroadcast, double duration);

        bool StopRecording();

        bool StartRecordingEarly();

        bool ExtendRecording();

        void Preview();

        List<PanoptoSession> GetUpcomingSessions();

        PanoptoSession GetCurrentSession();

        string GetRoomState();

        void Pause();

        void Resume();

        void Reconfigure();
    }
}