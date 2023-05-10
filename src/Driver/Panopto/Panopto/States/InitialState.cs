using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp.CrestronDataStore;
using Crestron.Panopto;
using CommandType = Crestron.Panopto.Command.CommandType;

namespace Crestron.Panopto
{
    public class InitialState : PanoptoState
    {
        private IDataStore _dataStore;
        private const string _userNameKey = "89a9a9592056458a9c9184531086007a";
        private const string _passwordKey = "9f27e7fea27241159c88d218b5bcdd2a";
        private const string _urlKey = "e843d5a71b7d461cadfbea31d38b9111";
        private const string _recorderNameKey = "d5523e8092f843afbc59210a54477f0f";
        private const string _recorderIPAddressKey = "251e2de5e9ee476ca93985ae95422428";
        private const string _recorderGuidKey = "90579540e41c4dcda338404306879a25";
        private int _onPage = 0;
        private int _recordersPerPage = 25;

        public InitialState()
        {
        }

        public InitialState(Panopto.Driver p, IDataStore dataStore)
        {
            _dataStore = dataStore;
            P = p;
        }

        public InitialState(Panopto.Driver p, IDataStore dataStore, bool deleteGuidKeyValue)
        {
            _dataStore = dataStore;
            if (deleteGuidKeyValue == true)
            {
                StoreGuid("");
            }
        }

        public override void HttpDataHandler(HttpsResponseArgs args)
        {


            PanoptoLogger.Notice("Panopto.InitialState.HttpDataHandler received response from Panopto");
            if (!string.IsNullOrEmpty(args.ResponseString))
            {
                PanoptoLogger.Notice(args.ResponseString);

                if (args.ResponseString.Contains("<LogOnWithPasswordResult>"))
                {
                    ProcessLogOnWithPasswordResponse(args);
                }
                else if (args.ResponseString.Contains("Fault"))
                {
                    ProcessLogOnWithPasswordResponse(args);
                }
                else if (args.ResponseString.Contains("<ListRecordersResult"))
                {
                    ProcessListRecordersResponse(args);
                }
            }
            else
            {
                PanoptoLogger.Error("Login Failed: {0}", args.ResponseCode);
                args.Command.Ignore = true;
                P.SendResult(args.Command);
            }
        }

        private void ProcessLogOnWithPasswordResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.InitialState.ProcessLogOnWithPasswordResponse");
            Command command = args.Command;
            if (args.ResponseString.Contains("Fault"))
            {
                PanoptoLogger.Notice("Invalid URL");
                PanoptoFeedback pf = new PanoptoFeedback
                {
                    FeedbackMessageValue = DisplayMessageEnum.UnableToLogIn,
                    Transitioning = false
                };
                P.ChangeFeedback(pf);
                command.Ignore = true;
            }
            else
            {
                if (args.ResponseString.Contains("true"))
                {
                    PanoptoLogger.Notice("Login accepted");
                    _onPage = 0;
                    string cookie = args.Headers["Set-Cookie"].ToString();
                    cookie = cookie.Substring(0, (cookie.IndexOf(';')));
                    P.Cookie = cookie;
                    PanoptoLogger.Notice("Cookie is '{0}'", cookie);

                    PanoptoFeedback pf = new PanoptoFeedback
                    {
                        FeedbackMessageValue = DisplayMessageEnum.AccessingRemoteRecorder,
                        Transitioning = true
                    };
                    P.ChangeFeedback(pf);

                    P.API.ListRecorders(P, 0, _recordersPerPage, CommandType.Poll, this);
                }
                else if (args.ResponseString.Contains("false"))
                {
                    PanoptoLogger.Notice("Login rejected");
                    PanoptoFeedback pf = new PanoptoFeedback
                    {
                        FeedbackMessageValue = DisplayMessageEnum.UnableToLogIn,
                        Transitioning = false
                    };
                    P.ChangeFeedback(pf);
                }
                command.Ready = true;
            }
            P.SendResult(command);
        }

        private void ProcessListRecordersResponse(HttpsResponseArgs args)
        {
            PanoptoLogger.Notice("Panopto.InitialState.ProcessListRecordersResponse");
            Command command = args.Command;
            PanoptoLogger.Notice("Searching for remote recorder {0}", P.RecorderName);
            bool foundRecorder = false;
            string remoteRecorderXML = string.Format("<a:Name>{0}</a:Name>", P.RecorderName);
            PanoptoLogger.Notice("Looking for {0}", remoteRecorderXML);
            if (args.ResponseString.Contains(remoteRecorderXML))
            {
                if (!string.IsNullOrEmpty(P.IpAddress))
                {
                    if (args.ResponseString.Contains(P.IpAddress))
                    {
                        foundRecorder = true;
                    }
                }
                else
                {
                    foundRecorder = true;
                }

                if (foundRecorder)
                {
                    PanoptoLogger.Notice("Found remote recorder {0}", P.RecorderName);

                    string guid = GetRemoteRecorderGuid(args.ResponseString);
                    StoreGuid(guid);
                    P.RecorderId = guid;

                    if (StoreConfiguration(P.UserName, P.Password, P.PanoptoUrl, P.RecorderName, P.IpAddress))
                    {
                        ChangeState(new IdleState(P), false, DisplayMessageEnum.AbleToAccessRemoteRecorder);
                    }
                }
                else
                {
                    PanoptoLogger.Notice("Have not yet found the remote recorder");
                    PanoptoFeedback pf = new PanoptoFeedback()
                    {
                        Transitioning = true,
                        FeedbackMessageValue = DisplayMessageEnum.AccessingRemoteRecorder
                    };
                    P.ChangeFeedback(pf);
                }
            }

            if (!foundRecorder)
            {
                int recorderCount = GetRecorderCount(args.ResponseString);
                int pageCount = recorderCount / _recordersPerPage;
                int remainder = recorderCount % _recordersPerPage;
                if (remainder > 0)
                {
                    pageCount++;
                }
                if (_onPage < pageCount)
                {
                    _onPage++;
                    //the queue will not allow the command to be added in since there is a command in the queue with this command name already
                    //So instead of sending ignore call clear queue to empty the queue out and then add this command to the queue
                    P.ClearQueue();
                    P.API.ListRecorders(P, _onPage, _recordersPerPage, CommandType.Poll, this);
                }
                else
                {
                    PanoptoLogger.Notice("Did not find the remote recorder");
                    PanoptoFeedback pf = new PanoptoFeedback()
                    {
                        Transitioning = false,
                        FeedbackMessageValue = DisplayMessageEnum.UnableToAccessRemoteRecorder
                    };
                    P.ChangeFeedback(pf);
                    P.ClearQueue();
                }
            }
            else
            {
                //if the RemoteRecorderPreviewData recorder was found don't clear the queue just move to the next command
                command.Ready = true;
                P.SendResult(command);
            }
        }

        private int GetRecorderCount(string response)
        {
            PanoptoLogger.Notice("Panopto.InitialState.GetRecorderCount");
            int result = 0;

            string[] tokens = response.Split('<');
            for (int onToken = 0; onToken < tokens.Length; onToken++)
            {
                string token = tokens[onToken];
                if (token.Contains(string.Format("a:TotalResultCount>")))
                {
                    string value = token.Replace("a:TotalResultCount>", string.Empty);
                    PanoptoLogger.Notice("Number of recorders is {0}", value);
                    result = Convert.ToInt32(value);
                    break;
                }
            }

            return result;
        }

        private string GetRemoteRecorderGuid(string response)
        {
            string guid = string.Empty;

            string[] tokens = response.Split('<');
            for (int onToken = 0; onToken < tokens.Length; onToken++)
            {
                string token = tokens[onToken];
                if (token.Contains(string.Format("a:Name>{0}", P.RecorderName)))
                {
                    int guidIndex = onToken - 4;
                    if (guidIndex >= 0)
                    {
                        guid = tokens[guidIndex].Replace("a:Id>", string.Empty);
                        PanoptoLogger.Notice("Remote Recorder Guid is {0}", guid);
                        break;
                    }
                }
            }

            return guid;
        }

        public override string GetStateName()
        {
            return StateConsts.InitialState;
        }

        //this state doesn't do any polling so there is no need to override the StopState method

        private void StoreGuid(string guid)
        {
            PanoptoLogger.Notice("Storing guid");
            StoreValue(guid, _recorderGuidKey);
        }

        public override void TryAutoConfigure()
        {
            if (GetStoredConfiguration(out P.UserName, out P.Password, out P.PanoptoUrl, out P.RecorderName, out P.IpAddress))
            {
                P.ChangeFeedback(new PanoptoFeedback()
                {
                    Transitioning = true,
                    FeedbackMessageValue = DisplayMessageEnum.LoggingIn
                });
                
                P.API.LogOnWithPassword(P, P.UserName, P.Password, P.PanoptoUrl, CommandType.Control, this);
            }
        }

        private bool StoreConfiguration(string userName, string password, string url, string recorderName, string ipAddress)
        {
            bool configurationStored = false;
            PanoptoLogger.Notice("Storing username");
            if (StoreValue(userName, _userNameKey))
            {
                PanoptoLogger.Notice("Storing password");
                if (StoreValue(EncryptPassword(password), _passwordKey))
                {
                    PanoptoLogger.Notice("Storing url");
                    if (StoreValue(url, _urlKey))
                    {
                        PanoptoLogger.Notice("Storing recorder name");
                        if (StoreValue(recorderName, _recorderNameKey))
                        {
                            PanoptoLogger.Notice("Storing ip address");
                            if (StoreValue(ipAddress, _recorderIPAddressKey))
                            {
                                configurationStored = true;
                            }
                        }
                    }
                }
            }
            return configurationStored;
        }

        private string EncryptPassword(string password)
        {
            var passwordBytes = Encoding.ASCII.GetBytes(password);
            var encryptedPassword = Convert.ToBase64String(passwordBytes);
            return encryptedPassword;
        }

        private string DecryptPassword(string encryptedPassword)
        {
            var decryptedPassword = string.Empty;
            var decryptedBytes = Convert.FromBase64String(encryptedPassword);
            decryptedPassword = Encoding.ASCII.GetString(decryptedBytes, 0, decryptedBytes.Length);
            return decryptedPassword;
        }

        private bool GetStoredConfiguration(out string username, out string password, out string url, out string recorderName, out string ipAddress)
        {
            PanoptoLogger.Notice("Checking for a stored configuration");
            var retrieveSuccess = false;
            username = string.Empty;
            password = string.Empty;
            url = string.Empty;
            recorderName = string.Empty;
            ipAddress = String.Empty;
            PanoptoLogger.Notice("Retrieving username");
            if (GetStoredValue(out username, _userNameKey))
            {
                PanoptoLogger.Notice("Retrieving password");
                if (GetStoredValue(out password, _passwordKey))
                {
                    password = DecryptPassword(password);
                    PanoptoLogger.Notice("Retrieving url");
                    if (GetStoredValue(out url, _urlKey))
                    {
                        PanoptoLogger.Notice("Retrieving recordername");
                        if (GetStoredValue(out recorderName, _recorderNameKey))
                        {
                            PanoptoLogger.Notice("Retrieving ipAddress");
                            if (GetStoredValue(out ipAddress, _recorderIPAddressKey))
                            {
                                retrieveSuccess = true;
                            }
                        }
                    }
                }
            }
            return retrieveSuccess;
        }

        private bool GetStoredValue(out string value, string key)
        {
            var stringType = typeof(string);
            value = string.Empty;
            object retrievedObject;
            var retrieveSuccess = false;
            var errorCode = _dataStore.GetLocalValue(key, stringType, out retrievedObject);
            if (errorCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
            {
                retrieveSuccess = true;
                value = retrievedObject.ToString();
            }
            else
            {
                PanoptoLogger.Error("Unable to retrieve value, error: {0}", errorCode.ToString());
            }
            return retrieveSuccess;
        }

        private bool StoreValue(string value, string key)
        {
            var storedSuccess = false;
            var errorCode = _dataStore.SetLocalValue(key, value);
            if (errorCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
            {
                storedSuccess = true;
            }
            else
            {
                PanoptoLogger.Error("Unable to store value, error: {0}", errorCode.ToString());
            }
            return storedSuccess;
        }
    }
}