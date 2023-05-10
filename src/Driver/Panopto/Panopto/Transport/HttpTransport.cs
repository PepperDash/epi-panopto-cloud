using System;
using System.Text;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;
using CommandName = Crestron.Panopto.Command.CommandName;
using CommandProtocol = Crestron.Panopto.Command.CommandProtocol;
using CommandType = Crestron.Panopto.Command.CommandType;
using Crestron.Panopto.Common.BasicDriver;
using System.Collections.Generic;

namespace Crestron.Panopto
{
    class HttpTransport
    {
        private HttpsClient _client = new HttpsClient();
        public Action<HttpsResponseArgs> HttpDataHandler;
        public Action<HttpsResponseArgs> HttpDownloadDataHandler;
        private Panopto.Driver _p;
        private ThreadSafeQueue _controlCommands = new ThreadSafeQueue("ControlCommands");
        private ThreadSafeQueue _pollingCommands = new ThreadSafeQueue("PollingCommands");
        TimelineEventHandler timeline;
        private int _repeatCount = 0;
        private bool _driverBusy = false;
        private bool _serverBusy = false;
        private int _messageSentTick = 0;
        private int _serverSaidBusyTick = 0;

        private const int _waitForServerTime = 1000;
        private const int _resendCommandInterval = 500;
        private const int _commandTimeout = 9000;
        //three tries on when going to repeat and it is the fourth time then the command fails and a message is logged
        //if this is a control command then this means something is probably wrong with the driver so throw the disconnected flag to notify the user of te problem
        private const int _maxRepeatCount = 4;
        private bool _queueCleared = false;

        public HttpTransport(Panopto.Driver p)
        {
            _p = p;
            timeline = new TimelineEventHandler(_resendCommandInterval, 0);
            timeline.Start(0, true);
            timeline.EventExecute += PrepareToSendNextCommand;
            short adapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);
            string macAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, adapterId);
            _client.UserAgent = macAddress;
        }

        private string BuildContent(string body, string envelopeHeader)
        {
            PanoptoLogger.Notice("Panopto.HttpTransport.BuildContent");
            StringBuilder message = new StringBuilder();
            if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(envelopeHeader))
            {
                message.Append(string.Format("<x:Envelope {0}>\r", envelopeHeader));
                message.Append("<x:Header/>\r");
                message.Append("<x:Body>\r");
                message.Append(string.Format("{0}\r", body));
                message.Append("</x:Body>\r");
                message.Append("</x:Envelope>");
            }
            return message.ToString();
        }

        public void ClearQueue()
        {
            _controlCommands.Clear();
            _pollingCommands.Clear();
            _serverBusy = false;
            _driverBusy = false;
            _queueCleared = true;
        }

        private void SetServerBusy()
        {
            //the timeline will check this tick against the server busy wait time if server busy is true then send the message again once it is greater or equal
            _serverSaidBusyTick = CrestronEnvironment.TickCount;
            _serverBusy = true;
        }

        public void QueueMessage(Command command)
        {
            PanoptoLogger.Notice("Panopto.HttpTransport.QueueMessage");
            if (command.State == _p.GetRoomState())
            {
                PanoptoLogger.Notice("command is for the current state of the driver");
                switch (command.Type)
                {
                    case CommandType.Control:
                        {
                            _controlCommands.Add(command);
                            break;
                        }
                    case CommandType.Poll:
                        {
                            _pollingCommands.Add(command);
                            break;
                        }
                }
            }
            else
            {
                PanoptoLogger.Notice("command is not for the current state of the driver");
            }
        }

        private Command GetNextCommand()
        {
            Command command = null;

            PanoptoLogger.Notice("Getting next command");
            command = _controlCommands.GetAt(0);
            if (command is Command)
            {
                PanoptoLogger.Notice("Got command from control queue");
            }
            else
            {
                command = _pollingCommands.GetAt(0);
                if (command is Command)
                {
                    PanoptoLogger.Notice("Got command from polling queue");
                }
                else
                {
                    PanoptoLogger.Notice("No commands queued");
                }
            }

            return command;
        }

        private void PrepareToSendNextCommand()
        {
            PanoptoLogger.Notice("Panopto.HttpTransport.PrepareToSendNextCommand");

            if (_serverBusy)
            {
                PanoptoLogger.Notice("Server busy");
                if (CheckServerTime())
                {
                    //the server sent a Process_Busy message and the transport has waited the prescribed ammount of time
                    //the server sent no follow up response which resulted in this timeout getting aborted and it has now lapsed
                    PanoptoLogger.Notice("Server Busy");
                    Command command = GetNextCommand();
                    if (command is Command)
                    {
                        PanoptoLogger.Notice("Resend Command {0}", command.Name);
                        SendMessage(command);
                    }
                }
            }
            else
            {
                PanoptoLogger.Notice("Server ready _driverBusy is {0}", _driverBusy);
                if (!_driverBusy)
                {
                    PanoptoLogger.Notice("Driver ready to send next message");
                    Command command = GetNextCommand();
                    if (command is Command)
                    {
                        PanoptoLogger.Notice("command.Name is {0}", command.Name);
                        SendMessage(command);
                        _queueCleared = false;
                    }
                }
                else
                {
                    PanoptoLogger.Notice("Driver is busy");
                    if (CheckMessageTime())
                    {
                        _repeatCount++;
                        PanoptoLogger.Notice("Repeat count is {0}", _repeatCount);
                        if (_repeatCount < _maxRepeatCount)
                        {
                            Command command = GetNextCommand();
                            if (command is Command)
                            {
                                PanoptoLogger.Notice("Resend Command {0}", command.Name);
                                if (command is Command)
                                {
                                    SendMessage(command);
                                }
                                else
                                {
                                    PanoptoLogger.Notice("command is not Command");
                                }
                            }
                        }
                        else
                        {
                            PanoptoLogger.Notice("The message has timed out {0} times", _maxRepeatCount);
                            var pf = new PanoptoFeedback()
                            {
                                ServerDisconnect = true
                            };
                            _p.ChangeFeedback(pf);
                        }
                    }
                }
            }
        }

        private bool CheckMessageTime()
        {
            bool result = false;

            CheckTime(_messageSentTick, _commandTimeout);

            return result;
        }

        private bool CheckServerTime()
        {
            bool result = false;

            CheckTime(_serverSaidBusyTick, _waitForServerTime);

            return result;
        }

        private bool CheckTime(int sentTick, int timeTick)
        {
            bool result = false;

            int currentTick = CrestronEnvironment.TickCount;
            int timedOutTick = sentTick + timeTick;
            if (currentTick >= timedOutTick)
            {
                result = true;
            }

            return result;
        }

        private void SendMessage(Command command)
        {
            //Do not try to switch states from this method!
            PanoptoLogger.Notice("Panopto.HttpTransport.SendMessage");
            try
            {
                _p.StateChangeLock.Enter();
                if (command.State == _p.GetRoomState())
                {
                    switch (command.Protocol)
                    {
                        case Command.CommandProtocol.Web:
                            {
                                SendHtmlRequest(command);
                                break;
                            }
                        case Command.CommandProtocol.Rest:
                            {
                                SendRestMessage(command);
                                break;
                            }
                        case Command.CommandProtocol.Soap:
                            {
                                SendSoapMessage(command);
                                break;
                            }
                    }
                    _messageSentTick = CrestronEnvironment.TickCount;
                }
                else
                {
                    PanoptoLogger.Notice("{0} message is not for the current state so it is being ignored", command.Name);
                    command.Ignore = true;
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.HttpTransport Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
            finally
            {
                _p.StateChangeLock.Leave();
            }
        }

        private void SendHtmlRequest(Command command)
        {
            //This executes entirely inside a CCritical lock
            //Do only what is required
            //Do not try to switch states from this method!
            if (command is Command && command.Protocol == CommandProtocol.Web)
            {
                PanoptoLogger.Notice("Panopto.HttpTransport.SendHtmlRequest");
                HttpsClientRequest request = new HttpsClientRequest();
                request.Url.Parse(command.Url);
                request.Header.AddHeader(new HttpsHeader("Cookie", _p.Cookie.Substring(_p.Cookie.IndexOf('.'))));
                SetServerBusy();
                ProcessResponseCode(_client.DispatchAsyncEx(request, DownloadHostResponseCallback, command));
            }
        }

        private void SendRestMessage(Command command)
        {
            //This executes entirely inside a CCritical lock
            //Do only what is required
            //Do not try to switch states from this method!
            PanoptoLogger.Notice("HttpsClient UserAgent is {0}", _client.UserAgent);
            if (command is Command && command.Protocol == CommandProtocol.Rest)
            {
                PanoptoLogger.Notice("Panopto.HttpTransport.SendRestMessage");
                HttpsClientRequest request = new HttpsClientRequest();
                request.Url.Parse(command.Url);
                request.RequestType = command.RequestType;
                request.Header.AddHeader(new HttpsHeader("Content-Type", "application/json"));
                request.Header.AddHeader(new HttpsHeader("Cookie", _p.Cookie.Substring(_p.Cookie.IndexOf('.'))));
                PanoptoLogger.Notice(string.Format("TX: url is '{0}'\rheader is {1}", request.Url, request.Header));
                SetServerBusy();
                ProcessResponseCode(_client.DispatchAsyncEx(request, HostResponseCallback, command));
            }
        }

        private void SendSoapMessage(Command command)
        {
            //This executes entirely inside a CCritical lock
            //Do only what is required
            //Do not try to switch states from this method!
            PanoptoLogger.Notice("HttpsClient UserAgent is {0}", _client.UserAgent);
            try
            {
                if (command is Command && command.Protocol == CommandProtocol.Soap)
                {
                    PanoptoLogger.Notice("Panopto.HttpTransport.SendSoapMessage");
                    HttpsClientRequest request = new HttpsClientRequest();
                    request.ContentString = BuildContent(command.Body, command.EnvelopeHeader);
                    RequestType requestType = RequestType.Post;
                    request.Header.ContentType = "text/xml; charset=utf-8";
                    request.Header.AddHeader(new HttpsHeader("SOAPAction", command.SoapAction));
                    request.Url.Parse(command.Url);
                    request.RequestType = requestType;
                    request.KeepAlive = false;
                    request.Encoding = Encoding.UTF8;
                    PanoptoLogger.Notice(string.Format("TX: url is '{0}'\rheader is '{1}'\rContentString is:\r{2}", request.Url, request.Header, request.ContentString));
                    _client.Url.Parse(command.Url);
                    SetServerBusy();
                    ProcessResponseCode(_client.DispatchAsyncEx(request, HostResponseCallback, command));
                }
            }
            catch (Exception ex)
            {
                PanoptoLogger.Error("Panopto.HttpTransport.SendSoapMessage Error Command.Name was {0} Message is {1}", command.Name, ex);
                var pf = new PanoptoFeedback()
                {
                    ServerDisconnect = true
                };
                _p.ChangeFeedback(pf);
            }
            PanoptoLogger.Notice("SendMessage method completed");
        }

        private void ProcessResponseCode(HttpsClient.DISPATCHASYNC_ERROR responseCode)
        {
            //This executes entirely inside a CCritical lock
            //Do only what is required
            //Do not try to switch states from this method!
            PanoptoLogger.Notice("Panopto.HttpTransport.ProcessResponseCode responseCode is '{0}'", responseCode);
            switch (responseCode)
            {
                case HttpsClient.DISPATCHASYNC_ERROR.PENDING:
                    {
                        //this allows the server to tell the transport if it is ready too
                        //A pending response will eventually get a response
                        PanoptoLogger.Notice("Server Response Pending");
                        break;
                    }
                case HttpsClient.DISPATCHASYNC_ERROR.THREAD_ERROR:
                case HttpsClient.DISPATCHASYNC_ERROR.PROCESS_BUSY:
                    {
                        PanoptoLogger.Notice("Server busy");
                        //Busy response code means the server probably is not going to respond back so give the server a moment and then send the command again
                        break;
                    }
                default:
                    {
                        PanoptoLogger.Notice("Server response code is ready");
                        //a unknown error occured. sending read and marking the server not busy to attempt to move on to the next command if it isn't a fatal error
                        //if either of the above codes where received previously and the result comes in with a code that isn't busy or pending then this will clear the server busy flag
                        _serverBusy = false;
                        break;
                    }
            }
        }

        public void DownloadHostResponseCallback(HttpsClientResponse response, HTTPS_CALLBACK_ERROR Error, object args)
        {
            PanoptoLogger.Notice("Panopto.HttpTransport.DownloadHostResponseCallback");
            PanoptoLogger.Notice("response.ResponseUrl is {0}", response.ResponseUrl);

            Command command = (Command)args;

            if (response is HttpsClientResponse && !string.IsNullOrEmpty(response.ContentString))
            {
                if (HttpDownloadDataHandler != null)
                {
                    PanoptoLogger.Notice("HttpDataHandler != null");
                    HttpsResponseArgs responseArgs = new HttpsResponseArgs()
                    {
                        ResponseString = response.ContentString,
                        ResponseBytes = response.ContentBytes,
                        ResponseCode = response.Code,
                        Headers = response.Header,
                        Url = response.ResponseUrl,
                        Command = command
                    };

                    //the image downloaded so mark the server as not busy
                    _serverBusy = false;
                    HttpDownloadDataHandler(responseArgs);
                }
            }
        }

        public void HostResponseCallback(HttpsClientResponse response, HTTPS_CALLBACK_ERROR Error, object args)
        {
            PanoptoLogger.Notice("Panopto.HttpTransport.HostResponseCallback");

            Command command = (Command)args;

            if (response is HttpsClientResponse)
            {
                if (!string.IsNullOrEmpty(response.ContentString))
                {
                    PanoptoLogger.Notice("RX:\n{0}", response.ContentString);
                }
                PanoptoLogger.Notice("response.Code is {0}", response.Code);
            }

            switch (Error)
            {
                case HTTPS_CALLBACK_ERROR.COMPLETED:
                    {
                        PanoptoLogger.Notice("Completed");
                        //if the server sent a pending or Process_Busy and then responded this is where that flag is cleared
                        _serverBusy = false;
                        break;
                    }
                case HTTPS_CALLBACK_ERROR.INVALID_PARAM:
                case HTTPS_CALLBACK_ERROR.UNKNOWN_ERROR:
                    {
                        PanoptoLogger.Notice("Error response from server");
                        var pf = new PanoptoFeedback()
                        {
                            ServerDisconnect = true
                        };
                        _p.ChangeFeedback(pf);
                        break;
                    }
            }

            if (response is HttpsClientResponse)
            {
                PanoptoLogger.Notice("Panopto.HttpTransport response is HttpsClientResponse");
                if (HttpDataHandler != null)
                {
                    PanoptoLogger.Notice("HttpDataHandler != null");
                    HttpsResponseArgs responseArgs = new HttpsResponseArgs()
                    {
                        ResponseString = response.ContentString,
                        ResponseBytes = response.ContentBytes,
                        ResponseCode = response.Code,
                        Headers = response.Header,
                        Url = response.ResponseUrl,
                        Command = command
                    };
                    HttpDataHandler(responseArgs);
                }
            }
        }

        public void ResultReceived(Command command)
        {
            try
            {
                PanoptoLogger.Notice("Panopto.HttpTransport.ResultReceived");
                if (!_queueCleared)
                {
                    if (command is Command)
                    {
                        PanoptoLogger.Notice("command is Command");
                        PanoptoLogger.Notice("Name is {0} Type is {1} Ready is {2} Ignore is {3}", command.Name, command.Type, command.Ready, command.Ignore);
                        if (command.Ready || command.Ignore)
                        {
                            switch (command.Type)
                            {
                                case CommandType.Control:
                                    {
                                        PanoptoLogger.Notice("Removing {0} command from _controlCommands", command.Name);
                                        _controlCommands.RemoveItem(command);
                                        break;
                                    }
                                case CommandType.Poll:
                                    {
                                        PanoptoLogger.Notice("Removing {0} command from _pollingCommands", command.Name);
                                        _pollingCommands.RemoveItem(command);
                                        break;
                                    }
                            }
                            _driverBusy = false;
                        }
                    }
                    else
                    {
                        PanoptoLogger.Notice("command is not a instance of Command");
                    }
                }
                else
                {
                    PanoptoLogger.Notice("No result expected. Ignoring result.");
                    _driverBusy = false;
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.HttpTransport.ResultReceived Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
                PanoptoLogger.Error("Stacktrace: {0}", e.StackTrace);
            }
        }
    }
}
