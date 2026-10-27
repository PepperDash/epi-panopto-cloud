using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Xml.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;

namespace PepperDash.Essentials.Plugins
{
    /// <summary>
    /// Finds the session a remote recorder is recording, through Panopto's SOAP API.
    /// </summary>
    /// <remarks>
    /// <para>The REST API this plugin otherwise uses cannot answer "what is this recorder recording
    /// now": its remote-recorder endpoints are for Panopto's hardware partners only. So a recording
    /// this device did not start itself — a class booked in Panopto, or one already running when
    /// the program restarted — left it with no session id, and with no title, end time, Stop or
    /// Extend. The SOAP API lists a recorder's scheduled sessions, and their states, to an ordinary
    /// account; Panopto's own developers point to it as the way to do this.</para>
    /// <para>Read-only. The id it finds is the one the REST <c>scheduledRecordings</c> endpoint
    /// takes, so everything after finding it is the path a recording started here already uses.
    /// Verified against upenn.hosted.panopto.com.</para>
    /// <para>Hand-written XML over the plugin's <see cref="HttpClient"/> rather than a generated
    /// WCF client: two calls do not justify the dependency.</para>
    /// </remarks>
    public class PanoptoSessionFinder
    {
        private const string ApiPath = "/Panopto/PublicAPI/4.6/";
        private const string ArraysNs = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";

        private readonly IKeyed _parent;
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;

        public PanoptoSessionFinder(IKeyed parent, HttpClient http, string baseUrl, string username, string password)
        {
            _parent = parent;
            _http = http;
            _baseUrl = baseUrl.TrimEnd('/');
            _username = username;
            _password = password;
        }

        /// <summary>
        /// The session the recorder is recording now, or null if there is none or it could not be
        /// asked.
        /// </summary>
        /// <remarks>
        /// Chosen by the session's own state rather than by its times: a session reads
        /// <c>Recording</c> (or <c>Broadcasting</c>) while it is being captured, so there is no
        /// guessing at the edges of a booking. Times are the fallback for a server that answers
        /// otherwise.
        /// </remarks>
        public FoundSession FindCurrentSession(Guid recorderId)
        {
            var ids = GetScheduledRecordings(recorderId);
            if (ids == null || ids.Count == 0)
                return null;

            var sessions = GetSessions(ids);
            if (sessions == null || sessions.Count == 0)
                return null;

            var now = DateTime.UtcNow;

            return sessions.FirstOrDefault(s => s.State == "Recording" || s.State == "Broadcasting")
                   ?? sessions.FirstOrDefault(s => s.StartTime <= now && now < s.EndTime);
        }

        private List<Guid> GetScheduledRecordings(Guid recorderId)
        {
            var body = string.Format(
                "<remoteRecorderIds xmlns:b=\"{0}\"><b:guid>{1}</b:guid></remoteRecorderIds>", ArraysNs, recorderId);

            var response = Call("RemoteRecorderManagement.svc", "IRemoteRecorderManagement", "GetRemoteRecordersById", body);
            if (response == null)
                return null;

            return response.Descendants()
                .Where(e => e.Name.LocalName == "ScheduledRecordings")
                .SelectMany(e => e.Elements())
                .Select(e => { Guid id; return Guid.TryParse(e.Value, out id) ? id : Guid.Empty; })
                .Where(id => id != Guid.Empty)
                .ToList();
        }

        private List<FoundSession> GetSessions(IEnumerable<Guid> ids)
        {
            var body = new StringBuilder();
            body.AppendFormat("<sessionIds xmlns:b=\"{0}\">", ArraysNs);
            foreach (var id in ids)
                body.AppendFormat("<b:guid>{0}</b:guid>", id);
            body.Append("</sessionIds>");

            var response = Call("SessionManagement.svc", "ISessionManagement", "GetSessionsById", body.ToString());
            if (response == null)
                return null;

            var sessions = new List<FoundSession>();

            foreach (var element in response.Descendants().Where(e => e.Name.LocalName == "Session"))
            {
                Guid id;
                if (!Guid.TryParse(Child(element, "Id"), out id))
                    continue;

                DateTime start;
                double duration;
                var hasStart = DateTime.TryParse(Child(element, "StartTime"), null,
                    System.Globalization.DateTimeStyles.AdjustToUniversal, out start);
                double.TryParse(Child(element, "Duration"), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out duration);

                sessions.Add(new FoundSession
                {
                    Id = id,
                    Name = Child(element, "Name"),
                    State = Child(element, "State"),
                    StartTime = hasStart ? start : DateTime.MaxValue,
                    EndTime = hasStart ? start.AddSeconds(duration) : DateTime.MinValue,
                });
            }

            return sessions;
        }

        private static string Child(XElement element, string name)
        {
            var child = element.Elements().FirstOrDefault(e => e.Name.LocalName == name);
            return child == null ? null : child.Value;
        }

        private XDocument Call(string service, string contract, string operation, string body)
        {
            var auth = string.Format(
                "<auth xmlns:a=\"http://schemas.datacontract.org/2004/07/Panopto.Server.Services.PublicAPI.V40\">" +
                "<a:Password>{0}</a:Password><a:UserKey>{1}</a:UserKey></auth>",
                SecurityElement.Escape(_password), SecurityElement.Escape(_username));

            var envelope = string.Format(
                "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body>" +
                "<{0} xmlns=\"http://tempuri.org/\">{1}{2}</{0}></s:Body></s:Envelope>",
                operation, auth, body);

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + ApiPath + service))
                {
                    request.Content = new StringContent(envelope, Encoding.UTF8, "text/xml");
                    request.Headers.Add("SOAPAction", string.Format("\"http://tempuri.org/{0}/{1}\"", contract, operation));

                    using (var response = _http.Send(request))
                    using (var reader = new StreamReader(response.Content.ReadAsStream()))
                    {
                        var content = reader.ReadToEnd();

                        if (!response.IsSuccessStatusCode)
                        {
                            // A SOAP fault arrives as a 500 with the reason in the body.
                            _parent.LogDebug("SOAP {0} returned {1}: {2}", operation, (int)response.StatusCode, content);
                            return null;
                        }

                        return XDocument.Parse(content);
                    }
                }
            }
            catch (Exception ex)
            {
                _parent.LogDebug("SOAP {0} failed: {1}", operation, ex.Message);
                return null;
            }
        }

        public class FoundSession
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
    }
}
