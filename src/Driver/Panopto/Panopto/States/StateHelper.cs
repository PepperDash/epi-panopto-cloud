using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;

namespace Crestron.Panopto
{
    static class StateHelper
    {
        public static RecorderState GetRemoteRecorderState(string response)
        {
            RecorderState state = RecorderState.Unknown;

            string[] tokens = response.Split('<');
            for (int onToken = 0; onToken < tokens.Length; onToken++)
            {
                string token = tokens[onToken];
                if (token.Contains("a:State>") && !token.Contains("/"))
                {
                    state = (RecorderState)Enum.Parse(typeof(RecorderState), token.Replace("a:State>", string.Empty), false);
                    PanoptoLogger.Notice("Panopto.StateHelper.GetRemoteRecorderState Recorder state is '{0}'", state);
                }
            }

            return state;
        }

        public static SessionState GetSessionState(string response)
        {
            SessionState state = SessionState.Unknown;

            string[] tokens = response.Split('<');
            for (int onToken = 0; onToken < tokens.Length; onToken++)
            {
                string token = tokens[onToken];
                if (token.Contains("a:State>") && !token.Contains("/"))
                {
                    state = (SessionState)Enum.Parse(typeof(SessionState), token.Replace("a:State>", string.Empty), false);
                    PanoptoLogger.Notice("Panopto.StateHelper.GetSessionState Recorder state is '{0}'", state);
                }
            }

            return state;
        }

        public static Guid GetNextSessionGuid(string response)
        {
            try
            {
                Guid guid = GetSessionGuids(response).ElementAt(0);
                return guid;
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.StateHelper.GetNextSessionGuid Error: {0}", e);
                return new Guid();
            }
        }

        public static List<Guid> GetSessionGuids(string response)
        {
            List<Guid> guids = new List<Guid>();
            try
            {
                

                string[] tokens = response.Split('<');
                for (int onToken = 0; onToken < tokens.Length; onToken++)
                {
                    string token = tokens[onToken];
                    if (token.Contains("b:guid>") && !token.Contains("/"))
                    {
                        Guid guid = new Guid(token.Replace("b:guid>", string.Empty));
                        PanoptoLogger.Notice("Scheduled Session Guid '{0}' Found", guid);
                        guids.Add(guid);
                    }
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.StateHelper.GetSessionGuids Error: {0}", e);
            }
            return guids;
        }

        public static bool ProcessScheduleRecordingResponse(string response)
        {
            bool result = false;

            if (!response.Contains("Fault") && response.Contains("<a:ConflictsExist>false</a:ConflictsExist>"))
            {
                result = true;
            }

            return result;
        }

        public static string ProcessGetDefaultFolderForRecorderResponse(string response)
        {
            string guid = string.Empty;
            PanoptoLogger.Notice("Panopto.StateHelper.ProcessGetDefaultFolderForRecorderResponse");
            string[] tokens = response.Split('<');
            foreach (string token in tokens)
            {
                if (token.Contains("GetDefaultFolderForRecorderResult>") && !token.Contains("/"))
                {
                    guid = token.Replace("GetDefaultFolderForRecorderResult>", string.Empty);
                    break;
                }
            }
            return guid;
        }

        public static PanoptoSession ProcessNextSessionByRecordingId(string response, Guid sessionId, Crestron.Panopto.Driver p)
        {
            PanoptoSession newSession = null;
            PanoptoLogger.Notice("Panopto.StateHelper.ProcessNextSessionByRecordingId");
            try
            {
                PanoptoLogger.Notice("Checking for recording id {0}", sessionId);
                if (response.Contains(string.Format("<a:Id>{0}</a:Id>", sessionId.ToString())))
                {
                    PanoptoLogger.Notice("found session");
                    newSession = new PanoptoSession();
                    newSession.RecordingId = sessionId;
                    string[] tokens = response.Split('<');
                    foreach (string token in tokens)
                    {
                        PanoptoLogger.Notice("Processing token '{0}'", token);
                        if (token.Contains("a:Duration>") && !token.Contains("/"))
                        {
                            newSession.Duration = Convert.ToDouble(token.Replace("a:Duration>", string.Empty));
                            PanoptoLogger.Notice("_nextSession.Duration is {0}", newSession.Duration);
                        }
                        if (token.Contains("a:IsBroadcast>") && !token.Contains("/"))
                        {
                            newSession.IsBroadcast = Convert.ToBoolean(token.Replace("a:IsBroadcast>", string.Empty));
                            PanoptoLogger.Notice("_nextSession.IsBroadcast is {0}", newSession.IsBroadcast);
                        }
                        if (token.Contains("a:Name>") && !token.Contains("/a:Name"))
                        {
                            //if we escape this instead of allowing the xpanel to do it the < and > characters will not be displayed so we are allowing the xpanel to handle it
                            //newSession.Name = token.Replace("a:Name>", string.Empty).Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");
                            newSession.Name = token.Replace("a:Name>", string.Empty);
                            PanoptoLogger.Notice("_nextSession.Name is {0}", newSession.Name);
                        }
                        if (token.Contains("a:StartTime>") && !token.Contains("/"))
                        {
                            newSession.StartTime = DateTime.Parse(token.Replace("a:StartTime>", string.Empty));
                            PanoptoLogger.Notice("_nextSession.StartTime is {0}", newSession.StartTime);
                        }
                        if (token.Contains("a:State>") && !token.Contains("/"))
                        {
                            newSession.State = (SessionState)Enum.Parse(typeof(SessionState), token.Replace("a:State>", string.Empty), false);
                            PanoptoLogger.Notice("_nextSession.State is {0}", newSession.State);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PanoptoLogger.Error("Panopto.StateHelper.ProcessNextSessionByRecordingId Error message is {0}", ex.Message);
            }

            if (newSession is PanoptoSession)
            {
                PanoptoLogger.Notice("newSession is PanoptoSession");
            }
            else
            {
                PanoptoLogger.Notice("did not find a session with id {0}", sessionId);
            }

            return newSession;
        }

        public static void Record(Crestron.Panopto.Driver p, PanoptoState state, string recordingName, DateTime startTime, double duration, bool isBroadcast)
        {
            DateTime endTime = startTime.AddSeconds(duration);
        }

        public static string XmlEscape(string message)
        {
            return message.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}