using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using RequestType = Crestron.SimplSharp.Net.Https.RequestType;
using Crestron.Panopto.Common.BasicDriver;

namespace Crestron.Panopto
{
    public class Command
    {
        public enum CommandType
        {
            Unknown = 0,
            Control = 1,
            Poll = 2
        }

        public enum CommandProtocol
        {
            Unknown = 0,
            Rest = 1,
            Soap = 2,
            Web = 3
        }

        public enum CommandName
        {
            Unknown = 0,
            PreviewVideo = 1,
            PreviewAudio = 2,
            Extend = 3,
            Stop = 4,
            Pause = 5,
            Resume = 6,
            GetPublicSessionId = 7,
            GetPreviewUrls = 8,
            LogOnWithPassword = 9,
            ListRecorders = 10,
            GetRemoteRecorderById = 11,
            GetSessionById = 12,
            GetDefaultFolderForRemoteRecorder = 13,
            ScheduleRecording = 14,
            StartSessionEarly = 15,
            StartSessionEarlyGetSessionById = 16,
            RescheduleSession = 17
        }

        public string StateName
        {
            get
            {
                return State.GetStateName();
            }
        }

        public PanoptoState State { get; private set; }
        public CommandType Type { get; set; }
        public CommandName Name { get; private set; }
        public CommandProtocol Protocol { get; private set; }
        public string Url { get; set; }
        public RequestType RequestType { get; private set; }
        public string SoapAction { get; private set; }
        public string EnvelopeHeader { get; private set; }
        public string Body { get; private set; }
        public bool Ready { get; set; }
        public bool Ignore { get; set; }

        public Command(string url, CommandName name, CommandType type, PanoptoState state)
        {
            Url = url;
            Protocol = CommandProtocol.Web;
            Name = name;
            Ready = false;
            Ignore = false;
            Type = type;
            State = state;
        }

        public Command(string url, RequestType requestType, CommandName name, CommandType type, PanoptoState state)
        {
            Url = url;
            RequestType = requestType;
            Protocol = CommandProtocol.Rest;
            Name = name;
            Ready = false;
            Ignore = false;
            Type = type;
            State = state;
        }

        public Command(string url, string soapAction, string envelopeHeader, string body, CommandName name, CommandType type, PanoptoState state)
        {
            Url = url;
            SoapAction = soapAction;
            EnvelopeHeader = envelopeHeader;
            Body = body;
            Protocol = CommandProtocol.Soap;
            Name = name;
            Ready = false;
            Ignore = false;
            Type = type;
            State = state;
        }
    }
}