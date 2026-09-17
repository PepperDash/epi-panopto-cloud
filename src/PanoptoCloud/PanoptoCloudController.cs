using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using Serilog.Events;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using RequestType = Crestron.SimplSharp.Net.Https.RequestType;

namespace PepperDash.Essentials.Plugins
{
    public class PanoptoCloudController : ReconfigurableBridgableDevice, ICommunicationMonitor,
        IHasRecordingControl, IHasRecordingInfo, IHasPolling
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        /// <summary>
        /// Set once the by-id read has proved it is not available on this server, after which
        /// every poll goes back to searching. See <see cref="GetRecorderById"/>.
        /// </summary>
        private bool _byIdUnavailable;

        /// <summary>True once a by-id read has actually worked, which is what makes a later 404
        /// mean "this recorder is gone" rather than "this server has no such endpoint".</summary>
        private bool _byIdWorked;

        private readonly CTimer _oauthTimer;
        private readonly CTimer _pollTimer;
        private readonly CTimer _recordingTimer;

        private readonly PanoptoCloudStatusMonitor _monitor;

        private string _token;
        private RecoderInfo _recorder = new RecoderInfo();

        private int _defaultLength = 90;
        private Guid _currentRecordingId;
        private string _currentRecordingName;
        private DateTime _currentRecordingStartTime;
        private DateTime _currentRecordingEndTime;

        public readonly IntFeedback RecorderStatusInt;
        public readonly StringFeedback RecorderStatusString;

        public readonly BoolFeedback IsRecording;
        public readonly BoolFeedback IsPaused;
        public readonly BoolFeedback IsOnline;
        public readonly StringFeedback NameFeedback;
        public readonly StringFeedback CurrentRecordingId;
        public readonly StringFeedback CurrentRecordingName;
        public readonly StringFeedback CurrentRecordingStartTime;
        public readonly StringFeedback CurrentRecordingEndTime;
        public readonly StringFeedback CurrentRecordingLength;
        public readonly StringFeedback CurrentRecordingMinutesRemaining;
        public readonly IntFeedback DefaultLength;
        public readonly BoolFeedback NextRecordingExists;
        public readonly HttpsClient Client;

        static PanoptoCloudController()
        {
            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    var splitString = s.Split(':');
                    var device =
                        DeviceManager.AllDevices.OfType<PanoptoCloudController>().
                            FirstOrDefault(x => x.Key.Equals(splitString[0], StringComparison.OrdinalIgnoreCase));

                    if (device == null)
                    {
                        CrestronConsole.ConsoleCommandResponse("Device not found");
                        return;
                    }

                    var clientId = splitString[1];
                    if (String.IsNullOrEmpty(clientId))
                    {
                        CrestronConsole.ConsoleCommandResponse("Client Id cannot be blank");
                        return;
                    }
                    device.SetClientId(clientId);

                }, "PANOPTOCLIENT", "Format: [Device_Key]:[Client_Id]", ConsoleAccessLevelEnum.AccessAdministrator);

            // Reading a recorder by id is unverified against a live Panopto server — the published
            // v1 summary lists only the search endpoint. This says which call the poll is actually
            // making, so finding out does not mean reading logs.
            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    var key = s.Trim();
                    var device =
                        DeviceManager.AllDevices.OfType<PanoptoCloudController>().
                            FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

                    if (device == null)
                    {
                        CrestronConsole.ConsoleCommandResponse("Device not found");
                        return;
                    }

                    CrestronConsole.ConsoleCommandResponse(
                        "Recorder id: {0}\r\nRead by id: {1}\r\n",
                        device._recorder == null ? "unknown" : device._recorder.Id.ToString(),
                        device._byIdUnavailable
                            ? "not available here - searching by name"
                            : device._byIdWorked
                                ? "working"
                                : "not tried yet");

                }, "PANOPTOPOLL", "Format: [Device_Key] - reports how this recorder is being read",
                ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    var splitString = s.Split(':');
                    var device =
                        DeviceManager.AllDevices.OfType<PanoptoCloudController>().
                            FirstOrDefault(x => x.Key.Equals(splitString[0], StringComparison.OrdinalIgnoreCase));

                    if (device == null)
                    {
                        CrestronConsole.ConsoleCommandResponse("Device not found");
                        return;
                    }

                    var clientSecret = splitString[1];
                    if (String.IsNullOrEmpty(clientSecret))
                    {
                        CrestronConsole.ConsoleCommandResponse("Client Secret cannot be blank");
                        return;
                    }
                    device.SetClientSecret(clientSecret);

                }, "PANOPTOSECRET", "Format: [Device_Key]:[Client_Secret]", ConsoleAccessLevelEnum.AccessAdministrator);
        }

        public PanoptoCloudController(DeviceConfig config)
            : base(config)
        {
            var props = config.Properties.ToObject<PanoptoCloudControllerProperties>();
            _url = props.Url;
            _username = props.Username;
            _password = props.Password;

            _oauthTimer = new CTimer(o => _token = String.Empty, Timeout.Infinite);

            _pollTimer = new CTimer(o => PollRecorder(), Timeout.Infinite);

            _recordingTimer = new CTimer(o => PollCurrentRecording(), Timeout.Infinite);

            _monitor = new PanoptoCloudStatusMonitor(this, 30000, 60000);

            IsRecording = new BoolFeedback(() => _recorder != null && (_recorder.State == RemoteRecorderState.Recording || _recorder.State == RemoteRecorderState.Paused));

            IsPaused = new BoolFeedback(() => _recorder != null && (_recorder.State == RemoteRecorderState.Paused));

            RecorderStatusInt = new IntFeedback(() => _recorder == null ? (int)RemoteRecorderState.Unknown : (int)_recorder.State);

            RecorderStatusString = new StringFeedback(() =>_recorder == null ? RemoteRecorderState.Unknown.ToString() : _recorder.State.ToString());

            CurrentRecordingStartTime = new StringFeedback(() => _currentRecordingId != Guid.Empty ? _currentRecordingStartTime.ToString("G") : String.Empty);

            CurrentRecordingEndTime = new StringFeedback(() => _currentRecordingId != Guid.Empty ? _currentRecordingEndTime.ToString("G") : String.Empty);

            CurrentRecordingName = new StringFeedback(() => _currentRecordingName);

            CurrentRecordingId = new StringFeedback(() => _currentRecordingId != Guid.Empty ? _currentRecordingId.ToString("D") : String.Empty);

            CurrentRecordingLength = new StringFeedback(() => _currentRecordingId.CurrentRecordingLength(_currentRecordingStartTime, _currentRecordingEndTime));

            CurrentRecordingMinutesRemaining = new StringFeedback(() => _currentRecordingId.CurrentRecordingTimeRemaining(_currentRecordingEndTime));

            DefaultLength = new IntFeedback(() => _defaultLength);

            NextRecordingExists = new BoolFeedback(() => false); // TODO [] implement next recording logic

            NameFeedback = new StringFeedback(() => Name);

            IsOnline = new BoolFeedback(() => _monitor.IsOnline);

            Client = new HttpsClient().WithDefaultSettings();

            Client.PeerVerification = false;
        }

        protected override bool CustomActivate()
        {
            RecorderStatusInt.OutputChange += (sender, args) =>
            {
                IsRecording.FireUpdate();
                IsPaused.FireUpdate();
                RecorderStatusString.FireUpdate();

                // RecorderStatusInt is the recorder own state, which is what RecordingState reads,
                // so this is the one place it can change.
                var recordingStateChanged = RecordingStateChanged;
                if (recordingStateChanged != null) recordingStateChanged(this, EventArgs.Empty);
            };

            CurrentRecordingEndTime.OutputChange += (sender, args) =>
            {
                CurrentRecordingLength.FireUpdate();
                CurrentRecordingMinutesRemaining.FireUpdate();
            };

            RecorderStatusString.OutputChange +=
                (sender, args) => this.LogInformation("Recorder Status:{0}", args.StringValue);

            IsRecording.FireUpdate();
            IsPaused.FireUpdate(); ;
            IsOnline.FireUpdate();
            RecorderStatusInt.FireUpdate();
            RecorderStatusString.FireUpdate();
            CurrentRecordingId.FireUpdate();
            CurrentRecordingName.FireUpdate();
            CurrentRecordingStartTime.FireUpdate();
            CurrentRecordingName.FireUpdate();
            DefaultLength.FireUpdate();
            NameFeedback.FireUpdate();

            return base.CustomActivate();
        }

        public void SetClientId(string clientId)
        {
            var key = Key + "-" + "ClientId";
            var storageResult = CrestronSecureStorage.Store(key,
                false,
                Encoding.ASCII.GetBytes(clientId),
                Encoding.ASCII.GetBytes(key));

            if (storageResult != eCrestronSecureStorageStatus.Ok)
            {
                this.LogInformation("Failed to store clientId");
                return;
            }

            CrestronSecureStorage.Flush();
            this.LogInformation("Successfully stored clientId");
        }

        public void SetClientSecret(string clientSecret)
        {
            var key = Key + "-" + "ClientSecret";
            var storageResult = CrestronSecureStorage.Store(key,
                false,
                Encoding.ASCII.GetBytes(clientSecret),
                Encoding.ASCII.GetBytes(key));

            if (storageResult != eCrestronSecureStorageStatus.Ok)
            {
                this.LogInformation("Failed to store clientSecret");
                return;
            }

            CrestronSecureStorage.Flush();
            this.LogInformation("Successfully stored clientSecret");
        }

        protected override void Initialize()
        {
            _pollTimer.Reset(5000, 10000);
        }

        public bool CheckTokenAndUpdate()
        {
            return !String.IsNullOrEmpty(_token) || UpdateToken();
        }

        public bool UpdateToken()
        {
            const string path = "/Panopto/oauth2/connect/token";
            var url = _url + path;
            try
            {
                string clientId;
                if (!Utils.TryGetValueFromSecureStorage(Key + "-" + "ClientId", out clientId))
                {
                    this.LogInformation("Client Id not set");
                    return false;
                }

                string clientSecret;
                if (!Utils.TryGetValueFromSecureStorage(Key + "-" + "ClientSecret", out clientSecret))
                {
                    this.LogInformation("Client Secret not set");
                    return false;
                }

                this.LogInformation("Getting token...");
                var token = PanoptoOauthClient.GetToken(url, _username, _password, clientId, clientSecret);
                _token = token.AccessToken;

                var expireTime = token.ExpiresIn * 1000 - 500;
                _oauthTimer.Reset(expireTime);
                this.LogInformation("Success!  Token expires at: {0}", DateTime.Now.AddMilliseconds(expireTime).ToShortTimeString());
                return true;
            }
            catch (Exception ex)
            {
                _oauthTimer.Reset();
                this.LogInformation("Caught an error getting the token: {0}{1}", ex.Message, ex.StackTrace);
                return false;
            }
        }

        public void SetDeviceName(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            Name = name;
            Config.Name = Name;
            SetConfig(Config);
            NameFeedback.FireUpdate();
            PollRecorder();
        }

        public void IncrementDefaultLength(ushort inc)
        {
            _defaultLength += inc;
            if (_defaultLength < 15)
            {
                _defaultLength = 15;
            }
            DefaultLength.FireUpdate();
        }

        public void DecrementDefaultLength(ushort dec)
        {
            _defaultLength -= dec;
            if (_defaultLength < 15)
            {
                _defaultLength = 15;
            }
            DefaultLength.FireUpdate();
        }

        public void SetDefaultLength(ushort value)
        {

            _defaultLength = value;
            if (_defaultLength < 15)
            {
                _defaultLength = 15;
            }
            DefaultLength.FireUpdate();
        }

        public bool PollRecorder()
        {
            if (!CheckTokenAndUpdate())
            {
                this.LogInformation("Cannot poll recorder; no token");
                return false;
            }

            if (String.IsNullOrEmpty(Name))
            {
                this.LogInformation("Cannot poll recorder, recorder name is not set");
                return false;
            }

            // Once the id is known, read that one recorder rather than searching for it by name
            // again. The search is the most expensive call in this API and this poll runs every
            // ten seconds — and more often than that while a command is being chased.
            var byId = _recorder != null && !_recorder.Id.Equals(Guid.Empty)
                ? GetRecorderById(_recorder.Id)
                : null;

            _recorder = byId ?? GetRecorder(Name, _url, _token) ?? new RecoderInfo();

            RecorderStatusInt.FireUpdate();

            this.LogInformation("Recorder Status:\r{0}", JsonConvert.SerializeObject(_recorder, Formatting.Indented));

            // True means the recorder was found. This used to return the opposite, which inverted
            // both callers: "if (id is empty && !PollRecorder()) return;" gave up precisely when
            // the poll had just succeeded, so a stop or extend issued before the first background
            // poll did nothing at all.
            return !_recorder.Id.Equals(Guid.Empty);
        }

        #region IHasRecordingControl / IHasRecordingInfo / IHasPolling

        /// <inheritdoc />
        public event EventHandler RecordingStateChanged;

        /// <inheritdoc />
        /// <remarks>
        /// Mapped from the remote recorder own state. Faulted, Blocked and Disconnected all become
        /// Offline: each means the recorder cannot be relied on to be capturing, and reporting Idle
        /// would say the room is simply not recording when in fact nobody knows. Previewing is
        /// Idle, since the recorder is live but committing nothing.
        /// </remarks>
        public eRecordingState RecordingState
        {
            get
            {
                if (_recorder == null) return eRecordingState.Offline;

                switch (_recorder.State)
                {
                    case RemoteRecorderState.Recording:
                        return eRecordingState.Recording;
                    case RemoteRecorderState.Paused:
                        return eRecordingState.Paused;
                    case RemoteRecorderState.Stopped:
                    case RemoteRecorderState.Previewing:
                        return eRecordingState.Idle;
                    default:
                        return eRecordingState.Offline;
                }
            }
        }

        /// <inheritdoc />
        public string RecordingTitle
        {
            get { return _currentRecordingName ?? String.Empty; }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Read from the stored time rather than parsed back out of CurrentRecordingEndTime, which
        /// formats for display and would have to be read back through whatever culture happens to
        /// be current.
        /// </remarks>
        public DateTime? RecordingEndTime
        {
            get { return _currentRecordingId == Guid.Empty ? (DateTime?)null : _currentRecordingEndTime; }
        }

        /// <inheritdoc />
        public int RecordingLengthMinutes
        {
            get { return _defaultLength; }
        }

        /// <inheritdoc />
        public void SetRecordingLength(int minutes)
        {
            SetDefaultLength((ushort)(minutes < 0 ? 0 : minutes));
        }

        /// <inheritdoc />
        /// <remarks>
        /// Reads the recorder and, when one is running, the recording itself. Both block on HTTP
        /// and share one client with this device own polling, so callers should not overlap them.
        /// </remarks>
        public void Poll()
        {
            PollRecorder();
            PollCurrentRecording();
        }

        #endregion

        public void StartRecording()
        {
            if (_recorder.Id.Equals(Guid.Empty))
                PollRecorder();

            if (_recorder.Id.Equals(Guid.Empty))
                return;

            const string path = "/Panopto/api/v1/scheduledRecordings?resolveConflicts=false";
            var url = String.Format("{0}{1}", _url, path);

            var body = new StartRecordingRequest
            {
                Name = Name + " " + DateTime.Now.ToString("g"),
                Description = Name + " " + DateTime.Now.ToString("g"),
                Recorders = new List<Recorder> { new Recorder { RemoteRecorderId = _recorder.Id } },
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(_defaultLength),
                FolderId = _recorder.DefaultRecordingFolder.Id
            };

            var request = GetDefaultRequestWithAuthHeaders(url, _token, RequestType.Post);

            request.ContentString = JsonConvert.SerializeObject(body);
            request.Header.AddHeader(new HttpsHeader("Content-Type", "application/json"));

            this.LogInformation("Attempting to start recording:{0}]\r{1}", request.Url.Url, request.ContentString);

                try
                {
                    Client.PeerVerification = false;
                    var result = Client.Dispatch(request);
                    ProcessCurrentRecording(result);
                }
                catch (Exception ex)
                {
                    this.LogInformation("Error starting recording {0}", ex.Message);
                }
            
        }

        public void StopRecording()
        {
            if (_recorder.Id.Equals(Guid.Empty) && !PollRecorder())
                return;

            if (_currentRecordingId == Guid.Empty)
            {
                this.LogInformation("Cannot stop recording, current recording id is not set");
                return;
            }

            const string path = "{0}/Panopto/api/v1/scheduledRecordings/{1}";
            var url = String.Format(path, _url, _currentRecordingId);

            var body = new
            {
                EndTime = DateTime.UtcNow,
            };

            var request = GetDefaultRequestWithAuthHeaders(url, _token, RequestType.Put);

            request.ContentString = JsonConvert.SerializeObject(body);
            request.Header.AddHeader(new HttpsHeader("Content-Type", "application/json"));

            this.LogInformation("Attempting to stop recording:{0}\r{1}", request.Url.Url, request.ContentString);

                try
                {
                    Client.PeerVerification = false;
                    var result = Client.Dispatch(request);
                    ProcessCurrentRecording(result);
                }
                catch (Exception ex)
                {
                    this.LogInformation("Error stopping recording {0}", ex.Message);
                }
            
        }

        public void PauseRecording()
        {
            throw new NotImplementedException();
        }

        public void ResumeRecording()
        {
            throw new NotImplementedException();
        }

        public void ExtendRecording()
        {
            const int defaultExtend = 15;
            ExtendRecording(defaultExtend);
        }

        public void ExtendRecording(int minutes)
        {
            if (_recorder.Id.Equals(Guid.Empty) && !PollRecorder())
                return;

            if (_currentRecordingId == Guid.Empty)
            {
                this.LogInformation("Cannot extend recording, current recording id is not set");
                return;
            }

            const string path = "{0}/Panopto/api/v1/scheduledRecordings/{1}?resolveConflicts=false";
            var url = String.Format(path, _url, _currentRecordingId);

            var request = GetDefaultRequestWithAuthHeaders(url, _token, RequestType.Put);
            var body = new
            {
                EndTime = _currentRecordingEndTime.AddMinutes(minutes)
            };

            request.ContentString = JsonConvert.SerializeObject(body);
            request.Header.AddHeader(new HttpsHeader("Content-Type", "application/json"));

            this.LogInformation("Attempting to extend recording:{0} {1}", request.Url.Url, request.ContentString);
                try
                {
                    Client.PeerVerification = false;
                    var result = Client.Dispatch(request);
                    ProcessCurrentRecording(result);
                }
                catch (Exception ex)
                {
                    this.LogInformation("Error extending recording {0}", ex.Message);
                }
           
        }

        public void PollCurrentRecording()
        {
            if (_recorder.Id == Guid.Empty && !PollRecorder())
                return;

            if (_currentRecordingId == Guid.Empty)
            {
                this.LogInformation("Cannot get recording, current recording id is not set");
                return;
            }

            const string path = "{0}/Panopto/api/v1/scheduledRecordings/{1}";
            var url = String.Format(path, _url, _currentRecordingId);

            var request = GetDefaultRequestWithAuthHeaders(url, _token, RequestType.Get);

            this.LogInformation("Polling current recording:{0}", request.Url.Url);

            try
            {
                Client.PeerVerification = false;
                var result = Client.Dispatch(request);
                if (result != null)
                {
                    ProcessCurrentRecording(result);
                }
            }
            catch (Exception ex)
            {
                this.LogInformation("Error polling recording {0}", ex.Message);
            }
            
        }

        public void ProcessCurrentRecording(HttpsClientResponse response)
        {
            if (response.Code != 200)
            {
                this.LogInformation("Error processing recording... Code:{0}\r{1}", response.Code, response.ContentString);
                _recordingTimer.Reset(5000);
            }
            else
            {
                {
                    var currentRecording = JsonConvert.DeserializeObject<ScheduledRecording>(response.ContentString);
                    this.LogDebug("Processing recording...\r{0}", JsonConvert.SerializeObject(currentRecording, Formatting.Indented));
                    this.LogDebug("Start time:{0}", currentRecording.StartTime.ToShortTimeString());
                    this.LogDebug("End time:{0}", currentRecording.EndTime.ToShortTimeString());

                    if (DateTime.UtcNow >= currentRecording.EndTime.ToUniversalTime())
                    {
                        this.LogInformation("Recording is over... clearing");
                        _currentRecordingId = Guid.Empty;
                        _currentRecordingName = String.Empty;
                        _recordingTimer.Stop();
                    }
                    else
                    {
                        _currentRecordingId = currentRecording.Id;
                        _currentRecordingName = currentRecording.Name;
                        _currentRecordingStartTime = currentRecording.StartTime;
                        _currentRecordingEndTime = currentRecording.EndTime;
                        _recordingTimer.Reset(5000);
                    }
                }
            }

            CurrentRecordingStartTime.FireUpdate();
            CurrentRecordingEndTime.FireUpdate();
            CurrentRecordingName.FireUpdate();
            CurrentRecordingId.FireUpdate();
        }

        /// <summary>
        /// Read one remote recorder by its id, or null to say "ask the other way".
        ///
        /// <para>Panopto's published v1 summary lists only <c>/remoteRecorders/search</c>, and the
        /// by-id path it does document — <c>/remoteRecorderAPI/remoteRecorder/{id}</c> — needs a
        /// hardware-partner key we do not have. This is the conventional REST sibling of the
        /// search endpoint and is <b>unverified against a live server</b>, so it is written to cost
        /// nothing if it is wrong: a 404 or 405 sets <see cref="_byIdUnavailable"/> and every poll
        /// from then on searches by name as before.</para>
        ///
        /// <para>Any other failure returns null too, which falls back for that cycle only — a
        /// timeout should not permanently give up a cheaper call.</para>
        /// </summary>
        public RecoderInfo GetRecorderById(Guid id)
        {
            if (_byIdUnavailable || id.Equals(Guid.Empty) || String.IsNullOrEmpty(_token))
                return null;

            var fullUrl = String.Format("{0}/Panopto/api/v1/remoteRecorders/{1}", _url, id);

            try
            {
                Client.PeerVerification = false;
                var request = GetDefaultRequestWithAuthHeaders(fullUrl, _token, RequestType.Get);
                var response = Client.Dispatch(request);

                if (response == null)
                    return null;

                // 405 is unambiguous: the path is there and GET is not allowed on it. A 404 is
                // not — it means "no such endpoint" only until a by-id read has once succeeded,
                // after which it means this recorder has been deleted or replaced, and the answer
                // is to search by name again rather than to give up on the cheap call forever.
                if (response.Code == 405 || (response.Code == 404 && !_byIdWorked))
                {
                    this.LogInformation(
                        "Reading a recorder by id is not available here (code {0}); using search from now on",
                        response.Code);
                    _byIdUnavailable = true;
                    return null;
                }

                if (response.Code == 404)
                {
                    this.LogInformation("Recorder {0} is no longer there; searching by name", id);
                    return null;
                }

                if (response.Code != 200)
                {
                    this.LogDebug("Recorder by id returned {0}; falling back to search", response.Code);
                    return null;
                }

                _monitor.SetOnlineStatus(true);
                IsOnline.FireUpdate();

                var recorder = JsonConvert.DeserializeObject<RecoderInfo>(response.ContentString);

                // A body that parses but carries no id is not this recorder — treat it as a miss
                // rather than overwriting a good one with an empty shell.
                if (recorder == null || recorder.Id.Equals(Guid.Empty))
                    return null;

                _byIdWorked = true;
                return recorder;
            }
            catch (Exception ex)
            {
                this.LogDebug("Error reading recorder by id, falling back to search: {0}", ex.Message);
                return null;
            }
        }

        public RecoderInfo GetRecorder(string name, string url, string token)
        {
            var defaultRecorderInfo = new RecoderInfo();
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(token))
                return defaultRecorderInfo;

            const string path = "/Panopto/api/v1/remoteRecorders/search";
            var fullUrl = String.Format("{0}{1}?searchQuery={2}", url, path, name);

            Debug.LogMessage(LogEventLevel.Information, "Searching for recorder name:{0}...", name);
 
                try
                {
                    Client.PeerVerification = false;
                    var request = GetDefaultRequestWithAuthHeaders(fullUrl, token, RequestType.Get);
                    var response = Client.Dispatch(request);

                    if (response != null)
                    {
                        var responseCode = response.Code;

                        _monitor.SetOnlineStatus(responseCode == 200 && responseCode != 401);
             
                        IsOnline.FireUpdate();

                        return ParseRecordingInfo(name, response);
                    }
                    _monitor.SetOnlineStatus(false);
                       IsOnline.FireUpdate();
                    return _recorder;
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Error searching for recorder {0}{1}", ex.Message, ex.StackTrace);
                    return defaultRecorderInfo;
                } 
            
        }

        public static RecoderInfo ParseRecordingInfo(string name, HttpsClientResponse response)
        {
            if (response == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, "Error response is null");
                return new RecoderInfo();
            }
            {
                var results = JsonConvert.DeserializeObject<RemoteRecoderSearchResult>(response.ContentString);
                return results.Results.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ??
                       new RecoderInfo();
            }
        }

        public static HttpsClientRequest GetDefaultRequestWithAuthHeaders(string url, string token, RequestType type)
        {
            var request = new HttpsClientRequest { RequestType = type };
            request.Header.AddHeader(new HttpsHeader("Authorization", "Bearer " + token));
            request.Url.Parse(url);
            return request;
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new PanoptoCloudControllerJoinMap(joinStart);
            if (bridge != null)
                bridge.AddJoinMap(Key, joinMap);

            const int defaultLengthIncrement = 5;

            trilist.SetSigTrueAction(joinMap.Start.JoinNumber, StartRecording);
            trilist.SetSigTrueAction(joinMap.Stop.JoinNumber, StopRecording);
            trilist.SetSigTrueAction(joinMap.Pause.JoinNumber, PauseRecording);
            trilist.SetSigTrueAction(joinMap.Resume.JoinNumber, ResumeRecording);
            trilist.SetSigTrueAction(joinMap.Extend.JoinNumber, ExtendRecording);
            trilist.SetSigTrueAction(joinMap.IncLength.JoinNumber, () => IncrementDefaultLength(defaultLengthIncrement));
            trilist.SetSigTrueAction(joinMap.DecLength.JoinNumber, () => DecrementDefaultLength(defaultLengthIncrement));
            trilist.SetUShortSigAction(joinMap.DefaultRecordingLength.JoinNumber, SetDefaultLength);
            trilist.SetStringSigAction(joinMap.RecorderName.JoinNumber, SetDeviceName);

            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.RecorderOnline.JoinNumber]);
            IsRecording.LinkInputSig(trilist.BooleanInput[joinMap.IsRecording.JoinNumber]);
            IsPaused.LinkInputSig(trilist.BooleanInput[joinMap.IsPaused.JoinNumber]);
            NextRecordingExists.LinkInputSig(trilist.BooleanInput[joinMap.NextRecordingExists.JoinNumber]);
            DefaultLength.LinkInputSig(trilist.UShortInput[joinMap.DefaultRecordingLength.JoinNumber]);
            NameFeedback.LinkInputSig(trilist.StringInput[joinMap.RecorderName.JoinNumber]);
            CurrentRecordingId.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingId.JoinNumber]);
            CurrentRecordingName.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingName.JoinNumber]);
            CurrentRecordingStartTime.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingStartTime.JoinNumber]);
            CurrentRecordingEndTime.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingEndTime.JoinNumber]);
            CurrentRecordingLength.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingLength.JoinNumber]);
            CurrentRecordingMinutesRemaining.LinkInputSig(trilist.StringInput[joinMap.CurrentRecordingMinutesRemaining.JoinNumber]);
        }

        public StatusMonitorBase CommunicationMonitor { get { return _monitor; } }

        public class PanoptoCloudControllerProperties 
        {
            public string Url { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
    }
}

