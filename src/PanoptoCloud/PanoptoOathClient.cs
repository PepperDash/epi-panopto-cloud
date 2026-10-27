using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Plugins
{
    public static class PanoptoOauthClient
    {
        /// <summary>
        /// Fetch an access token with the password grant.
        /// </summary>
        /// <remarks>
        /// The form is URL-encoded by <see cref="FormUrlEncodedContent"/>. It used to be assembled
        /// with string.Format, so a password containing <c>&amp;</c>, <c>+</c> or <c>=</c> was
        /// sent as something else and the token request failed.
        /// </remarks>
        public static TokenResponse GetToken(HttpClient client, string url, string username, string password, string clientId, string clientPassword)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Base64Encode(clientId + ":" + clientPassword));
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Grant_type", "password"),
                    new KeyValuePair<string, string>("Username", username),
                    new KeyValuePair<string, string>("Password", password),
                    new KeyValuePair<string, string>("Scope", "api"),
                });

                using (var response = client.Send(request))
                using (var reader = new StreamReader(response.Content.ReadAsStream()))
                {
                    var content = reader.ReadToEnd();

                    if (!response.IsSuccessStatusCode)
                        throw new Exception(string.Format("Error getting token: {0} {1}", (int)response.StatusCode, content));

                    return JsonConvert.DeserializeObject<TokenResponse>(content);
                }
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }
    }
}
