using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Plugins
{
    public class PanoptoCloudController : ReconfigurableBridgableDevice, ICommunicationMonitor,
        IHasRecordingControl, IHasRecordingInfo, IHasPolling
    {
        /// <summary>How long to wait for Panopto. It is a cloud service, so this is more forgiving
        /// than a device on the LAN — but finite, where the Crestron client waited a minute.</summary>
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

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

        /// <summary>
        /// Shared by every request this device makes.
        /// </summary>
        /// <remarks>
        /// System.Net.Http rather than the Crestron client, which handles one request at a time: the
        /// recorder poll, the recording timer, the room's reads after a command and the commands
        /// themselves all run on different threads, and this client is built for that.
        /// </remarks>
        private readonly HttpClient _http;

        private volatile string _token;
        private RecoderInfo _recorder = new RecoderInfo();

        /// <summary>1 while the recorder or the current recording is being read. See
        /// <see cref="PollRecorder"/>.</summary>
        private int _pollingRecorder;
        private int _pollingRecording;

        /// <summary>The last token problem logged, so a missing client id or an unreachable server
        /// is reported once rather than every ten seconds.</summary>
        private string _lastTokenProblem;

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

        static PanoptoCloudController()
        {
            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    string clientId;
                    var device = FindForConsole(s, out clientId);
                    if (device == null)
                        return;

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
                    string clientSecret;
                    var device = FindForConsole(s, out clientSecret);
                    if (device == null)
                        return;

                    if (String.IsNullOrEmpty(clientSecret))
                    {
                        CrestronConsole.ConsoleCommandResponse("Client Secret cannot be blank");
                        return;
                    }
                    device.SetClientSecret(clientSecret);

                }, "PANOPTOSECRET", "Format: [Device_Key]:[Client_Secret]", ConsoleAccessLevelEnum.AccessAdministrator);
        }

        /// <summary>
        /// Split "key:value" console input and find the device.
        /// </summary>
        /// <remarks>
        /// Splits on the first colon only. A client secret may contain colons, and splitting on all
        /// of them stored whatever came before the second one.
        /// </remarks>
        private static PanoptoCloudController FindForConsole(string input, out string value)
        {
            var parts = (input ?? String.Empty).Split(new[] { ':' }, 2);
            value = parts.Length > 1 ? parts[1].Trim() : String.Empty;

            var device =
                DeviceManager.AllDevices.OfType<PanoptoCloudController>().
                    FirstOrDefault(x => x.Key.Equals(parts[0].Trim(), StringComparison.OrdinalIgnoreCase));

            if (device == null)
                CrestronConsole.ConsoleCommandResponse("Device not found");

            return device;
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

            var handler = new HttpClientHandler
            {
                // Unchanged from the Crestron client this replaces, which had peer and host
                // verification off.
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _http = new HttpClient(handler) { Timeout = RequestTimeout };
        }

        protected override bool CustomActivate()
        {
            RecorderStatusInt.OutputChange += (sender, args) =>
            {
                IsRecording.FireUpdate();
                IsPaused.FireUpdate();
                RecorderStatusString.FireUpdate();

                // RecorderStatusInt is the recorder own state, which is what RecordingState reads.
                RaiseRecordingStateChanged();
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
            _monitor.StatusChange += OnMonitorStatusChange;
            _monitor.Start();

            _pollTimer.Reset(5000, 10000);
        }

        /// <summary>
        /// Say once when Panopto stops or starts answering, and forget the recorder's state once
        /// it has been silent long enough to be called offline.
        ///
        /// <para>The poll keeps the last good answer through a failed request, so one slow reply
        /// does not flicker the panel to "not responding". That makes this the place stale state
        /// is dropped.</para>
        /// </summary>
        private void OnMonitorStatusChange(object sender, MonitorStatusChangeEventArgs args)
        {
            IsOnline.FireUpdate();

            switch (args.Status)
            {
                case MonitorStatus.IsOk:
                    this.LogInformation("Panopto is answering");
                    break;
                case MonitorStatus.InWarning:
                    this.LogWarning("Panopto is not answering");
                    break;
                case MonitorStatus.InError:
                    this.LogWarning("Panopto is offline; clearing the recorder's last state");
                    _recorder = new RecoderInfo();
                    RecorderStatusInt.FireUpdate();
                    break;
            }
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
                    ReportTokenProblem("Client Id not set");
                    return false;
                }

                string clientSecret;
                if (!Utils.TryGetValueFromSecureStorage(Key + "-" + "ClientSecret", out clientSecret))
                {
                    ReportTokenProblem("Client Secret not set");
                    return false;
                }

                this.LogDebug("Getting token...");
                var token = PanoptoOauthClient.GetToken(_http, url, _username, _password, clientId, clientSecret);
                _token = token.AccessToken;

                var expireTime = token.ExpiresIn * 1000 - 500;
                _oauthTimer.Reset(expireTime);

                _lastTokenProblem = null;
                this.LogDebug("Token expires at: {0}", DateTime.Now.AddMilliseconds(expireTime).ToShortTimeString());
                return true;
            }
            catch (Exception ex)
            {
                _oauthTimer.Reset();
                ReportTokenProblem("Error getting the token: " + ex.Message);
                return false;
            }
        }

        private void ReportTokenProblem(string problem)
        {
            if (problem == _lastTokenProblem)
            {
                this.LogDebug("{0}", problem);
                return;
            }

            _lastTokenProblem = problem;
            this.LogWarning("{0}", problem);
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

        /// <summary>
        /// Read the remote recorder. True means it was found.
        /// </summary>
        /// <remarks>
        /// Single-flight: the ten-second timer, the room's reads after a command and a command
        /// that needs the recorder can all arrive at once, and a read queued behind another only
        /// brings back what the first just read. A caller that arrives mid-read gets the answer as
        /// it stands.
        /// </remarks>
        public bool PollRecorder()
        {
            if (System.Threading.Interlocked.Exchange(ref _pollingRecorder, 1) == 1)
                return !_recorder.Id.Equals(Guid.Empty);

            try
            {
                return ReadRecorder();
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error polling recorder");
                return !_recorder.Id.Equals(Guid.Empty);
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _pollingRecorder, 0);
            }
        }

        private bool ReadRecorder()
        {
            if (!CheckTokenAndUpdate())
            {
                // Without a token there is no talking to Panopto, which is offline as far as this
                // room is concerned — whether the server is down or the credentials are missing.
                _monitor.SetOnlineStatus(false);
                return false;
            }

            if (String.IsNullOrEmpty(Name))
            {
                this.LogDebug("Cannot poll recorder, recorder name is not set");
                return false;
            }

            // Once the id is known, read that one recorder rather than searching for it by name
            // again. The search is the most expensive call in this API and this poll runs every
            // ten seconds — and more often than that while a command is being chased.
            var byId = _recorder != null && !_recorder.Id.Equals(Guid.Empty)
                ? GetRecorderById(_recorder.Id)
                : null;

            var found = byId ?? GetRecorder(Name, _url, _token);

            if (found == null)
            {
                // The read failed rather than finding nothing. Keep what we knew: replacing it
                // with an empty recorder turned every dropped request into Offline and back, and
                // threw away the id. The monitor clears it if this goes on.
                return !_recorder.Id.Equals(Guid.Empty);
            }

            _recorder = found;

            RecorderStatusInt.FireUpdate();

            this.LogVerbose("Recorder Status:\r{0}", JsonConvert.SerializeObject(_recorder, Formatting.Indented));

            return !_recorder.Id.Equals(Guid.Empty);
        }

        #region IHasRecordingControl / IHasRecordingInfo / IHasPolling

        /// <inheritdoc />
        /// <remarks>
        /// Also raised when the current recording's title or end time changes, not only the state.
        /// <see cref="IHasRecordingInfo"/> has no event of its own, so without this an extend, or a
        /// recording this device has only just learned about, reached the room only when something
        /// else happened to make it look.
        /// </remarks>
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
        public void Poll()
        {
            PollRecorder();
            PollCurrentRecording();
        }

        #endregion

        private void RaiseRecordingStateChanged()
        {
            var handler = RecordingStateChanged;
            if (handler == null)
                return;

            try
            {
                handler(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // A subscriber throwing must not stop the poll that raised this.
                this.LogError(ex, "Error raising RecordingStateChanged");
            }
        }

        public void StartRecording()
        {
            if (!CheckTokenAndUpdate())
            {
                this.LogWarning("Cannot start recording; no token");
                return;
            }

            if (_recorder.Id.Equals(Guid.Empty) && !PollRecorder())
            {
                this.LogWarning("Cannot start recording; recorder '{0}' not found", Name);
                return;
            }

            const string path = "/Panopto/api/v1/scheduledRecordings?resolveConflicts=false";
            var url = String.Format("{0}{1}", _url, path);

            var folder = _recorder.DefaultRecordingFolder;

            var body = new StartRecordingRequest
            {
                Name = Name + " " + DateTime.Now.ToString("g"),
                Description = Name + " " + DateTime.Now.ToString("g"),
                Recorders = new List<Recorder> { new Recorder { RemoteRecorderId = _recorder.Id } },
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(_defaultLength),
                FolderId = folder != null ? folder.Id : Guid.Empty
            };

            this.LogInformation("Attempting to start recording");

            try
            {
                ProcessCurrentRecording(Send(HttpMethod.Post, url, _token, body));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error starting recording");
            }
        }

        public void StopRecording()
        {
            if (!CheckTokenAndUpdate())
            {
                this.LogWarning("Cannot stop recording; no token");
                return;
            }

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

            this.LogInformation("Attempting to stop recording {0}", _currentRecordingId);

            try
            {
                ProcessCurrentRecording(Send(HttpMethod.Put, url, _token, body));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error stopping recording");
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
            if (!CheckTokenAndUpdate())
            {
                this.LogWarning("Cannot extend recording; no token");
                return;
            }

            if (_recorder.Id.Equals(Guid.Empty) && !PollRecorder())
                return;

            if (_currentRecordingId == Guid.Empty)
            {
                this.LogInformation("Cannot extend recording, current recording id is not set");
                return;
            }

            const string path = "{0}/Panopto/api/v1/scheduledRecordings/{1}?resolveConflicts=false";
            var url = String.Format(path, _url, _currentRecordingId);

            var body = new
            {
                EndTime = _currentRecordingEndTime.AddMinutes(minutes)
            };

            this.LogInformation("Attempting to extend recording {0} by {1} minutes", _currentRecordingId, minutes);

            try
            {
                ProcessCurrentRecording(Send(HttpMethod.Put, url, _token, body));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error extending recording");
            }
        }

        /// <summary>Read the recording this device started. Single-flight, like
        /// <see cref="PollRecorder"/>.</summary>
        public void PollCurrentRecording()
        {
            if (System.Threading.Interlocked.Exchange(ref _pollingRecording, 1) == 1)
                return;

            try
            {
                if (_recorder.Id == Guid.Empty && !PollRecorder())
                    return;

                if (_currentRecordingId == Guid.Empty)
                {
                    this.LogVerbose("No current recording to read");
                    return;
                }

                if (!CheckTokenAndUpdate())
                    return;

                const string path = "{0}/Panopto/api/v1/scheduledRecordings/{1}";
                var url = String.Format(path, _url, _currentRecordingId);

                this.LogVerbose("Polling current recording:{0}", url);

                ProcessCurrentRecording(Send(HttpMethod.Get, url, _token, null));
            }
            catch (Exception ex)
            {
                this.LogDebug("Error polling recording {0}", ex.Message);
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _pollingRecording, 0);
            }
        }

        public void ProcessCurrentRecording(PanoptoResponse response)
        {
            var before = RecordingInfoSignature();

            if (response == null)
            {
                // Nothing came back. Try again shortly, as for any other failure.
                _recordingTimer.Reset(5000);
            }
            else if (response.Code == 404)
            {
                // The session is gone. This used to fall into the branch below and ask again every
                // five seconds for as long as the processor stayed up.
                this.LogInformation("Recording {0} no longer exists; clearing", _currentRecordingId);
                ClearCurrentRecording();
            }
            else if (response.Code != 200)
            {
                this.LogDebug("Error processing recording... Code:{0}\r{1}", response.Code, response.Content);
                _recordingTimer.Reset(5000);
            }
            else
            {
                var currentRecording = JsonConvert.DeserializeObject<ScheduledRecording>(response.Content);
                this.LogVerbose("Processing recording...\r{0}", JsonConvert.SerializeObject(currentRecording, Formatting.Indented));

                if (DateTime.UtcNow >= currentRecording.EndTime.ToUniversalTime())
                {
                    this.LogInformation("Recording is over... clearing");
                    ClearCurrentRecording();
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

            CurrentRecordingStartTime.FireUpdate();
            CurrentRecordingEndTime.FireUpdate();
            CurrentRecordingName.FireUpdate();
            CurrentRecordingId.FireUpdate();

            if (RecordingInfoSignature() != before)
                RaiseRecordingStateChanged();
        }

        private void ClearCurrentRecording()
        {
            _currentRecordingId = Guid.Empty;
            _currentRecordingName = String.Empty;
            _recordingTimer.Stop();
        }

        /// <summary>What <see cref="IHasRecordingInfo"/> reports, as one value to compare.</summary>
        private string RecordingInfoSignature()
        {
            return String.Join("|", _currentRecordingId, _currentRecordingName, _currentRecordingEndTime.Ticks);
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

            var response = Send(HttpMethod.Get, fullUrl, _token, null);

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

            try
            {
                var recorder = JsonConvert.DeserializeObject<RecoderInfo>(response.Content);

                // A body that parses but carries no id is not this recorder — treat it as a miss
                // rather than overwriting a good one with an empty shell.
                if (recorder == null || recorder.Id.Equals(Guid.Empty))
                    return null;

                _byIdWorked = true;
                return recorder;
            }
            catch (Exception ex)
            {
                this.LogDebug("Unreadable recorder by id, falling back to search: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Search for the recorder by name.
        /// </summary>
        /// <returns>The recorder; an empty <see cref="RecoderInfo"/> when the search worked and
        /// found no such recorder; or null when the search itself failed. The last two used to be
        /// the same answer, so a failed request wiped the recorder.</returns>
        public RecoderInfo GetRecorder(string name, string url, string token)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(token))
                return null;

            const string path = "/Panopto/api/v1/remoteRecorders/search";
            var fullUrl = String.Format("{0}{1}?searchQuery={2}", url, path, Uri.EscapeDataString(name));

            this.LogVerbose("Searching for recorder name:{0}...", name);

            var response = Send(HttpMethod.Get, fullUrl, token, null);

            if (response == null || response.Code != 200)
            {
                _monitor.SetOnlineStatus(false);
                return null;
            }

            _monitor.SetOnlineStatus(true);

            try
            {
                return ParseRecordingInfo(name, response.Content);
            }
            catch (Exception ex)
            {
                this.LogDebug("Unreadable recorder search: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>Pick the named recorder out of a search result. Null when the body is not a
        /// search result; an empty <see cref="RecoderInfo"/> when it is and the recorder is not
        /// in it.</summary>
        public static RecoderInfo ParseRecordingInfo(string name, string content)
        {
            var results = JsonConvert.DeserializeObject<RemoteRecoderSearchResult>(content);

            if (results == null || results.Results == null)
                return null;

            return results.Results.FirstOrDefault(r => r.Name != null && r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ??
                   new RecoderInfo();
        }

        /// <summary>
        /// Send a request to Panopto and read the reply in full, or null if nothing came back.
        /// </summary>
        /// <remarks>
        /// A 401 drops the token, so the next call fetches a new one rather than failing until the
        /// expiry timer would have — a token can be revoked or expire early on the server's side.
        /// </remarks>
        private PanoptoResponse Send(HttpMethod method, string url, string token, object body)
        {
            try
            {
                using (var request = new HttpRequestMessage(method, url))
                {
                    if (!String.IsNullOrEmpty(token))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    if (body != null)
                    {
                        var json = JsonConvert.SerializeObject(body);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        this.LogVerbose("{0} {1}\r{2}", method, url, json);
                    }

                    using (var response = _http.Send(request))
                    using (var reader = new StreamReader(response.Content.ReadAsStream()))
                    {
                        var result = new PanoptoResponse((int)response.StatusCode, reader.ReadToEnd());

                        if (result.Code == 401)
                        {
                            this.LogDebug("Panopto refused the token; fetching a new one next time");
                            _token = String.Empty;
                        }

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                // Debug, not Error: an unreachable server fails every request of every poll, and the
                // communication monitor already says so once when it goes offline.
                this.LogDebug("{0} {1} failed: {2}", method, url, ex.Message);
                return null;
            }
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
