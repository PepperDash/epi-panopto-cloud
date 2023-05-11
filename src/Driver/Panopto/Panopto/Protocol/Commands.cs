using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto;
using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp.Net.Https;
using Driver = Crestron.Panopto.Driver;
using CommandName = Crestron.Panopto.Command.CommandName;
using CommandType = Crestron.Panopto.Command.CommandType;

namespace Crestron.Panopto
{
    public class Commands
    {
        private string _authUrlSuffix = "/Panopto/PublicAPI/4.2/Auth.svc?wsdl";
        //private string _authUrlSuffix = "/Panopto/PublicAPISSL/4.0/Auth.svc";
        private string _remoteRecorderManagementUrlSuffix = "/panopto/publicapi/4.2/remoterecordermanagement.svc";
        //private string _remoteRecorderManagementUrlSuffix = "/Panopto/PublicAPISSL/4.0/RemoteRecorderManagement.svc";
        private string _sessionManagementUrlSuffix = "/Panopto/PublicAPI/4.2/SessionManagement.svc";
        //private string _sessionManagementUrlSuffix = "Panopto/PublicAPISSL/4.0/SessionManagement.svc";

        public void LogOnWithPassword(Panopto.Driver p, string userName, string password, string url, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.LogOnWithPassword {0} with password {1}", userName, password);
            string envelopeHeader = "xmlns:x=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\"";
            string soapAction = "http://tempuri.org/IAuth/LogOnWithPassword";
            url = string.Format("{0}{1}", url, _authUrlSuffix);
            PanoptoLogger.Notice("url is '{0}'", url);

            StringBuilder body = new StringBuilder();
            body.Append("<tem:LogOnWithPassword>\r");
            body.Append(string.Format("<tem:userKey>{0}</tem:userKey>\r", userName));
            body.Append(string.Format("<tem:password>{0}</tem:password>\r", password));
            body.Append("</tem:LogOnWithPassword>");
            p.QueueMessage(new Command(url, soapAction, envelopeHeader, body.ToString(), CommandName.LogOnWithPassword, type, state));
        }

        public void ListRecorders(Panopto.Driver p, int page, int recordersPerPage, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.ListRecorders");
            try
            {
                string envelopeHeader = "xmlns:x=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\" xmlns:pan=\"http://schemas.datacontract.org/2004/07/Panopto.Server.Services.PublicAPI.V40\"";
                string soapAction = "http://tempuri.org/IRemoteRecorderManagement/ListRecorders";
                string url = string.Format("{0}{1}", p.PanoptoUrl, _remoteRecorderManagementUrlSuffix);
                PanoptoLogger.Notice("url is '{0}'", url);

                StringBuilder body = new StringBuilder();
                body.Append("<tem:ListRecorders>\r");
                body.Append("<tem:auth>\r");
                body.Append(string.Format("<pan:Password>{0}</pan:Password>\r", p.Password));
                body.Append(string.Format("<pan:UserKey>{0}</pan:UserKey>\r", p.UserName));
                body.Append("</tem:auth>\r");
                body.Append("<tem:pagination>\r");
                body.Append(string.Format("<pan:MaxNumberResults>{0}</pan:MaxNumberResults>\r", recordersPerPage));
                body.Append(string.Format("<pan:PageNumber>{0}</pan:PageNumber>\r", page));
                body.Append("</tem:pagination>\r");
                body.Append("<tem:sortBy>Name</tem:sortBy>\r");
                body.Append("</tem:ListRecorders>");

                p.QueueMessage(new Command(url, soapAction, envelopeHeader, body.ToString(), CommandName.ListRecorders, type, state));
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Error while calling ListRecorders");
                PanoptoLogger.Error(e.Message);
                PanoptoLogger.Error(e.StackTrace);
                p.ChangeFeedback(new PanoptoFeedback { FeedbackMessageValue = DisplayMessageEnum.UnableToAccessRemoteRecorder, Transitioning= false });
            }
        }

        public void GetRemoteRecorderById(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.GetRemoteRecorderById");
            if (!string.IsNullOrEmpty(p.RecorderId))
            {
                string envelopeHeader = "xmlns:x=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\" xmlns:pan=\"http://schemas.datacontract.org/2004/07/Panopto.Server.Services.PublicAPI.V40\" xmlns:arr=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"";
                string soapAction = "http://tempuri.org/IRemoteRecorderManagement/GetRemoteRecordersById";
                string url = string.Format("{0}{1}", p.PanoptoUrl, _remoteRecorderManagementUrlSuffix);
                PanoptoLogger.Notice("url is '{0}'", url);

                StringBuilder body = new StringBuilder();
                body.Append("<tem:GetRemoteRecordersById>\r");
                body.Append("<tem:auth>\r");
                body.Append(string.Format("<pan:Password>{0}</pan:Password>\r", p.Password));
                body.Append(string.Format("<pan:UserKey>{0}</pan:UserKey>\r", p.UserName));
                body.Append("</tem:auth>\r");
                body.Append("<tem:remoteRecorderIds>\r");
                body.Append(string.Format("<arr:guid>{0}</arr:guid>\r", p.RecorderId));
                body.Append("</tem:remoteRecorderIds>\r");
                body.Append("</tem:GetRemoteRecordersById>");

                p.QueueMessage(new Command(url, soapAction, envelopeHeader, body.ToString(), CommandName.GetRemoteRecorderById, type, state));
            }
            else
            {
                PanoptoLogger.Notice("Aborting GetRemoteRecorderById because RecorderID is null or empty");
            }
        }

        public void StartSessionEarlyGetSessionById(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            GetSessionById(p, CommandName.StartSessionEarlyGetSessionById, type, state);
        }

        public void GetSessionById(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            GetSessionById(p, CommandName.GetSessionById, type, state);
        }

        public void GetSessionById(Panopto.Driver p, CommandName name, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.GetSessionById");
            string envelopeHeader = "xmlns:x=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\" xmlns:pan=\"http://schemas.datacontract.org/2004/07/Panopto.Server.Services.PublicAPI.V40\" xmlns:arr=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"";
            string soapAction = "http://tempuri.org/ISessionManagement/GetSessionsById";
            string url = string.Format("{0}{1}", p.PanoptoUrl, _sessionManagementUrlSuffix);
            PanoptoLogger.Notice("url is '{0}'", url);

            StringBuilder body = new StringBuilder();
            body.Append("<tem:GetSessionsById>\r");
            body.Append("<tem:auth>\r");
            body.Append(string.Format("<pan:Password>{0}</pan:Password>\r", p.Password));
            body.Append(string.Format("<pan:UserKey>{0}</pan:UserKey>\r", p.UserName));
            body.Append("</tem:auth>\r");
            body.Append("<tem:sessionIds>\r");
            body.Append(string.Format("<arr:guid>{0}</arr:guid>\r", p.ReportedSessionId));
            body.Append("</tem:sessionIds>\r");
            body.Append("</tem:GetSessionsById>");

            p.QueueMessage(new Command(url, soapAction, envelopeHeader, body.ToString(), name, type, state));
        }

        public void GetDefaultFolderForRemoteRecorder(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.GetDefaultFolderForRemoteRecorder");
            string envelopeHeader = "xmlns:x=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\" xmlns:pan=\"http://schemas.datacontract.org/2004/07/Panopto.Server.Services.PublicAPI.V40\"";
            string soapAction = "http://tempuri.org/IRemoteRecorderManagement/GetDefaultFolderForRecorder";
            string url = string.Format("{0}{1}", p.PanoptoUrl, _remoteRecorderManagementUrlSuffix);
            PanoptoLogger.Notice("url is '{0}'", url);

            StringBuilder body = new StringBuilder();
            body.Append("<tem:GetDefaultFolderForRecorder>\r");
            body.Append("<tem:auth>\r");
            body.Append(string.Format("<pan:Password>{0}</pan:Password>\r", p.Password));
            body.Append(string.Format("<pan:UserKey>{0}</pan:UserKey>\r", p.UserName));
            body.Append("</tem:auth>\r");
            body.Append(string.Format("<tem:remoteRecorderId>{0}</tem:remoteRecorderId>\r", p.RecorderId));
            body.Append("</tem:GetDefaultFolderForRecorder>");

            p.QueueMessage(new Command(url, soapAction, envelopeHeader, body.ToString(), CommandName.GetDefaultFolderForRemoteRecorder, type, state)); 
        }

        public void ScheduleRecording(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.ScheduleRecording");
            string envelopeHeader = "xmlns:x=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\" xmlns:pan=\"http://schemas.datacontract.org/2004/07/Panopto.Server.Services.PublicAPI.V40\"";
            string soapAction = "http://tempuri.org/IRemoteRecorderManagement/ScheduleRecording";
            string url = string.Format("{0}{1}", p.PanoptoUrl, _remoteRecorderManagementUrlSuffix);
            PanoptoLogger.Notice("url is '{0}'", url);

            DateTime uStartDate = p.RecordingConfig.StartTime.ToUniversalTime();
            DateTime uEndDate = p.RecordingConfig.EndTime.ToUniversalTime();
            
            string startYear = uStartDate.Year.ToString().PadLeft(4, '0');
            string sStartMonth = uStartDate.Month.ToString().PadLeft(2, '0');
            string sStartDay = uStartDate.Day.ToString().PadLeft(2, '0');
            string sStartHour = uStartDate.Hour.ToString().PadLeft(2, '0');
            string sStartMinute = uStartDate.Minute.ToString().PadLeft(2, '0');
            string sStartSecond = uStartDate.Second.ToString().PadLeft(2, '0');
            string sStartDate = string.Format("{0}-{1}-{2}T{3}:{4}:{5}", startYear, sStartMonth, sStartDay, sStartHour, sStartMinute, sStartSecond);

            string endYear = uEndDate.Year.ToString().PadLeft(4, '0');
            string sEndMonth = uEndDate.Month.ToString().PadLeft(2, '0');
            string sEndDay = uEndDate.Day.ToString().PadLeft(2, '0');
            string sEndHour = uEndDate.Hour.ToString().PadLeft(2, '0');
            string sEndMinute = uEndDate.Minute.ToString().PadLeft(2, '0');
            string sEndSecond = uEndDate.Second.ToString().PadLeft(2, '0');
            string sEndDate = string.Format("{0}-{1}-{2}T{3}:{4}:{5}", endYear, sEndMonth, sEndDay, sEndHour, sEndMinute, sEndSecond);
            
            StringBuilder body = new StringBuilder();
            body.Append("<tem:ScheduleRecording>\r");
            body.Append("<tem:auth>\r");
            body.Append(string.Format("<pan:Password>{0}</pan:Password>\r", p.Password));
            body.Append(string.Format("<pan:UserKey>{0}</pan:UserKey>\r", p.UserName));
            body.Append("</tem:auth>\r");
            body.Append(string.Format("<tem:name>{0}</tem:name>\r", p.RecordingConfig.RecordingName));
            body.Append(string.Format("<tem:folderId>{0}</tem:folderId>\r", p.RecordingConfig.FolderId.ToString()));
            body.Append(string.Format("<tem:isBroadcast>{0}</tem:isBroadcast>\r", p.RecordingConfig.IsBroadcast.ToString().ToLower()));
            body.Append(string.Format("<tem:start>{0}</tem:start>\r", sStartDate));
            body.Append(string.Format("<tem:end>{0}</tem:end>\r", sEndDate));
            body.Append("<tem:recorderSettings>\r");
            body.Append("<pan:RecorderSettings>\r");
            body.Append(string.Format("<pan:RecorderId>{0}</pan:RecorderId>\r", p.RecorderId));
            body.Append("<pan:SuppressPrimary>false</pan:SuppressPrimary>\r");
            body.Append("<pan:SuppressSecondary>false</pan:SuppressSecondary>\r");
            body.Append("</pan:RecorderSettings>\r");
            body.Append("</tem:recorderSettings>\r");
            body.Append("</tem:ScheduleRecording>");

            p.QueueMessage(new Command(url, soapAction, envelopeHeader, body.ToString(), CommandName.ScheduleRecording, type, state)); 
        }

        public void StartSessionEarly(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            UpdateRecordingTime(p, CommandName.StartSessionEarly, type, state);
        }

        public void StopRecording(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            UpdateRecordingTime(p, CommandName.Stop, type, state);
        }

        public void ExtendRecording(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            UpdateRecordingTime(p, CommandName.Extend, type, state);
        }

        public void RescheduleSession(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            UpdateRecordingTime(p, CommandName.RescheduleSession, type, state);
        }

        private void UpdateRecordingTime(Panopto.Driver p, CommandName name, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.UpdateRecordingTime");
            string envelopeHeader = "xmlns:x=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\" xmlns:pan=\"http://schemas.datacontract.org/2004/07/Panopto.Server.Services.PublicAPI.V40\"";
            string soapAction = "http://tempuri.org/IRemoteRecorderManagement/UpdateRecordingTime";
            string url = string.Format("{0}{1}", p.PanoptoUrl, _remoteRecorderManagementUrlSuffix);
            PanoptoLogger.Notice("url is '{0}'", url);

            DateTime uStartDate = p.RecordingConfig.StartTime.ToUniversalTime();
            string startYear = uStartDate.Year.ToString().PadLeft(4, '0');
            string sStartMonth = uStartDate.Month.ToString().PadLeft(2, '0');
            string sStartDay = uStartDate.Day.ToString().PadLeft(2, '0');
            string sStartHour = uStartDate.Hour.ToString().PadLeft(2, '0');
            string sStartMinute = uStartDate.Minute.ToString().PadLeft(2, '0');
            string sStartSecond = uStartDate.Second.ToString().PadLeft(2, '0');
            string sStartDate = string.Format("{0}-{1}-{2}T{3}:{4}:{5}", startYear, sStartMonth, sStartDay, sStartHour, sStartMinute, sStartSecond);

            DateTime uEndDate = p.RecordingConfig.EndTime.ToUniversalTime();
            string endYear = uEndDate.Year.ToString().PadLeft(4, '0');
            string sEndMonth = uEndDate.Month.ToString().PadLeft(2, '0');
            string sEndDay = uEndDate.Day.ToString().PadLeft(2, '0');
            string sEndHour = uEndDate.Hour.ToString().PadLeft(2, '0');
            string sEndMinute = uEndDate.Minute.ToString().PadLeft(2, '0');
            string sEndSecond = uEndDate.Second.ToString().PadLeft(2, '0');
            string sEndDate = string.Format("{0}-{1}-{2}T{3}:{4}:{5}", endYear, sEndMonth, sEndDay, sEndHour, sEndMinute, sEndSecond);

            StringBuilder body = new StringBuilder();
            body.Append("<tem:UpdateRecordingTime>\r");
            body.Append("<tem:auth>\r");
            body.Append(string.Format("<pan:Password>{0}</pan:Password>\r", p.Password));
            body.Append(string.Format("<pan:UserKey>{0}</pan:UserKey>\r", p.UserName));
            body.Append("</tem:auth>\r");
            body.Append(string.Format("<tem:sessionId>{0}</tem:sessionId>\r", p.RecordingConfig.RecordingId.ToString()));
            body.Append(string.Format("<tem:start>{0}</tem:start>\r", sStartDate));
            body.Append(string.Format("<tem:end>{0}</tem:end>\r", sEndDate));
            body.Append("</tem:UpdateRecordingTime>");

            p.QueueMessage(new Command(url, soapAction, envelopeHeader, body.ToString(), name, type, state)); 
        }

        public void GetPublicSessionId(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.GetPublicSessionId");
            string url = string.Format("{0}/PublicAPI/4.1/SessionMetadata?deliveryId={1}", p.PanoptoUrl, p.RecordingConfig.RecordingId.ToString());
            p.QueueMessage(new Command(url, RequestType.Get, CommandName.GetPublicSessionId, type, state));
        }

        public void GetPreviewUrls(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.GetPreviewUrls");
            string url = string.Format("{0}/PublicAPI/4.1/RemoteRecorderPreviewData?remoteRecorderId={1}", p.PanoptoUrl, p.RecorderId);
            p.QueueMessage(new Command(url, RequestType.Get, CommandName.GetPreviewUrls, type, state));
        }

        public void Pause(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.Pause");
            string url = string.Format("{0}/PublicAPI/4.1/Pause?sessionId={1}", p.PanoptoUrl, p.RecordingConfig.PublicSessionId.ToString());
            p.QueueMessage(new Command(url, RequestType.Post, CommandName.Pause, type, state));
        }

        public void Resume(Panopto.Driver p, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.Resume");
            TimeSpan pauseTimeSpan = DateTime.Now.Subtract(p.RecordingConfig.PauseTime);
            int durationInSeconds = (int)pauseTimeSpan.TotalSeconds;
            PanoptoLogger.Notice("durationInSeconds is {0}", durationInSeconds);
            string url = string.Format("{0}/PublicAPI/4.1/PauseDuration?sessionId={1}&pauseId={2}&durationSeconds={3}", p.PanoptoUrl,
                p.RecordingConfig.PublicSessionId.ToString(), p.RecordingConfig.PauseId.ToString(), durationInSeconds);
            p.QueueMessage(new Command(url, RequestType.Post, CommandName.Resume, type, state));
        }

        public void Download(Panopto.Driver p, string url, CommandType type, PanoptoState state)
        {
            PanoptoLogger.Notice("Panopto.Commands.Download url is '{0}'", url);
            Command.CommandName commandName = CommandName.Unknown;
            if (url.Contains("type=Video"))
            {
                commandName = CommandName.PreviewVideo;
            }
            else if (url.Contains("type=Audio"))
            {
                commandName = CommandName.PreviewAudio;
            }
            Command command = new Command(url, commandName, type, state);
            p.QueueMessage(command);
        }
    }
}