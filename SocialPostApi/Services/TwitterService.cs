using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SocialPostApi.Models.Responses;

namespace SocialPostApi.Services
{
    public class TwitterService : ITwitterService
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _accessToken;
        private readonly string _accessSecret;

        private readonly HttpClient _http;

        public TwitterService(IConfiguration config)
        {
            _consumerKey = config["Twitter:ConsumerKey"]!;
            _consumerSecret = config["Twitter:ConsumerSecret"]!;
            _accessToken = config["Twitter:AccessToken"]!;
            _accessSecret = config["Twitter:AccessSecret"]!;

            _http = new HttpClient();
        }

        // ====================================================================
        // MAIN METHOD: Upload Image + Tweet (V2)
        // ====================================================================
        public async Task<TwitterPostResponse> PostX(string message, string imageUrl)
        {
            var response = new TwitterPostResponse();
            // 1. Download Image
            byte[] imageBytes = await _http.GetByteArrayAsync(imageUrl);

            // 2. INIT Upload
            string uploadUrl = "https://upload.twitter.com/1.1/media/upload.json";
            var initResult = await SendForm(uploadUrl, "POST", new()
            {
                { "command", "INIT" },
                { "media_type", "image/jpeg" },
                { "total_bytes", imageBytes.Length.ToString() }
            });

            if (!initResult.success)
                return new TwitterPostResponse
                {
                    Success = false,
                    Raw = "INIT ERROR => " + initResult.response
                };

            string mediaId = initResult.json.RootElement.GetProperty("media_id_string").GetString()!;

            // 3. APPEND Upload (Multipart)
            var appendResult = await SendMultipart(uploadUrl, new()
            {
                { "command", "APPEND" },
                { "media_id", mediaId },
                { "segment_index", "0" }
            }, imageBytes);

            if (!appendResult.success)
                return new TwitterPostResponse
                {
                    Success = false,
                    Raw = "APPEND ERROR => " + appendResult.response
                };

            // 4. FINALIZE Upload
            var finalizeResult = await SendForm(uploadUrl, "POST", new()
            {
                { "command", "FINALIZE" },
                { "media_id", mediaId }
            });

            if (!finalizeResult.success)
                return new TwitterPostResponse
                {
                    Success = false,
                    Raw = "FINALIZE ERROR => " + finalizeResult.response
                };

            // 5. Post Tweet (V2)
            var tweetPayload = new
            {
                text = message,
                media = new { media_ids = new[] { mediaId } }
            };

            var tweetResult = await SendJson("https://api.twitter.com/2/tweets", "POST", tweetPayload);


            return new TwitterPostResponse
            {
                Success = tweetResult.success,
                TweetId = tweetResult.success ?
                    JsonDocument.Parse(tweetResult.response).RootElement.GetProperty("data").GetProperty("id").GetString()
                    : null,
                MediaId = mediaId,
                Text = message,
                Raw = tweetResult.response
            };
        }


        // ====================================================================
        // Helper: POST Form
        // ====================================================================
        private async Task<(bool success, string response, JsonDocument json)>
            SendForm(string url, string method, Dictionary<string, string> body)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuthHeader(url, "POST", body));
            req.Content = new FormUrlEncodedContent(body);

            var res = await _http.SendAsync(req);
            string text = await res.Content.ReadAsStringAsync();

            JsonDocument? json = null;
            try { json = JsonDocument.Parse(text); } catch { }

            return (res.IsSuccessStatusCode, text, json!);
        }


        // ====================================================================
        // Helper: POST Multipart
        // ====================================================================
        private async Task<(bool success, string response)>
            SendMultipart(string url, Dictionary<string, string> fields, byte[] fileBytes)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuthHeader(url, "POST"));

            var content = new MultipartFormDataContent();
            foreach (var kv in fields)
                content.Add(new StringContent(kv.Value), kv.Key);

            content.Add(new ByteArrayContent(fileBytes), "media", "image.jpg");

            req.Content = content;

            var res = await _http.SendAsync(req);
            string text = await res.Content.ReadAsStringAsync();

            return (res.IsSuccessStatusCode, text);
        }


        // ====================================================================
        // Helper: POST JSON
        // ====================================================================
        private async Task<(bool success, string response)>
            SendJson(string url, string method, object obj)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuthHeader(url, method));
            req.Content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            string text = await res.Content.ReadAsStringAsync();

            return (res.IsSuccessStatusCode, text);
        }


        // ====================================================================
        // OAuth 1.0a Signature Generation (Optimized)
        // ====================================================================
        private string BuildOAuthHeader(string url, string method, Dictionary<string, string>? body = null)
        {
            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var oauth = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", _consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", _accessToken },
                { "oauth_version", "1.0" }
            };

            // INIT + FINALIZE only
            body?.ToList().ForEach(x => oauth[x.Key] = x.Value);

            string paramString = string.Join("&",
                oauth.Select(k => $"{Uri.EscapeDataString(k.Key)}={Uri.EscapeDataString(k.Value)}")
            );

            string signatureBase =
                $"{method.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";

            string signingKey =
                $"{Uri.EscapeDataString(_consumerSecret)}&{Uri.EscapeDataString(_accessSecret)}";

            using var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
            string signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBase)));

            oauth["oauth_signature"] = signature;

            return string.Join(", ",
                oauth.Select(x => $"{x.Key}=\"{Uri.EscapeDataString(x.Value)}\""));
        }
    }
}
