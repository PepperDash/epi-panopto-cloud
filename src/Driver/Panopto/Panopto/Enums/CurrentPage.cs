using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto
{
    public enum CurrentPage
    {
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
}