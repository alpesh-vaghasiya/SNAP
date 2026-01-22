using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SocialPostApi.Models.Responses;

namespace SocialPostApi.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly HttpClient _http;
        private readonly string _pageId;
        private readonly string _pageAccessToken;

        public FacebookService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _pageId = config["Facebook:PageId"]!;
            _pageAccessToken = config["Facebook:PageAccessToken"]!;
        }

        public async Task<SocialPostResult> PublishPost(string imageUrl, string message)
        {
            // FB photo post endpoint
            var url =
                $"https://graph.facebook.com/v18.0/{_pageId}/photos" +
                $"?url={Uri.EscapeDataString(imageUrl)}" +
                $"&caption={Uri.EscapeDataString(message)}" +
                $"&access_token={_pageAccessToken}";

            var response = await _http.PostAsync(url, null);
            var raw = await response.Content.ReadAsStringAsync();

            var result = new SocialPostResult
            {
                Platform = "facebook",
                Raw = raw
            };

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                result.Message = "Facebook API Error";
                return result;
            }

            try
            {
                var json = System.Text.Json.JsonDocument.Parse(raw);
                result.PostId = json.RootElement.GetProperty("id").GetString();
                result.Message = "Post published successfully";
                result.Success = true;
            }
            catch
            {
                result.Success = true; // post succeeded even if ID missing
                result.Message = "Post published, but ID not found in response";
            }
            return result;
        }
        public async Task<string> GetPublicImageUrl(string originalUrl)
        {
            // If already public, return same
            if (originalUrl.StartsWith("http"))
                return originalUrl;

            // Convert local file ID → public endpoint
            return $"https://dev.projecttree.in/api/public-image/{originalUrl}";
        }
    }
}
