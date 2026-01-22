using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SocialPostApi.Models;
using SocialPostApi.Models.Responses;

namespace SocialPostApi.Services
{
    public class InstagramService : IInstagramService
    {
        private readonly HttpClient _http;
        private readonly string _accessToken;
        private readonly string _igUserId;
        private readonly string _igPageId;

        public InstagramService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _accessToken = config["Instagram:AccessToken"]!;
            _igUserId = config["Instagram:InstagramUserId"]!;
            _igPageId = config["Facebook:PageId"]!;
        }

        public async Task<SocialPostResult> PublishPost(string imageUrl, string caption)
        {
            var response = new SocialPostResult
            {
                Platform = "instagram_facebook"
            };
            // STEP 1: Create media container
            var createMediaUrl =
                $"https://graph.facebook.com/v18.0/{_igUserId}/media" +
                $"?image_url={Uri.EscapeDataString(imageUrl)}" +
                $"&caption={Uri.EscapeDataString(caption)}" +
                $"&access_token={_accessToken}";

            var mediaResponse = await _http.PostAsync(createMediaUrl, null);
            var mediaResponseText = await mediaResponse.Content.ReadAsStringAsync();

            if (!mediaResponse.IsSuccessStatusCode)
            {
                response.Success = false;
                response.Message = "Instagram Error (Create Media): " + mediaResponseText;
                response.Raw = mediaResponseText;
                return response;
            }

            // Try to parse the response
            var mediaResult = System.Text.Json.JsonSerializer.Deserialize<MediaResponse>(mediaResponseText);

            if (mediaResult == null || string.IsNullOrEmpty(mediaResult.id))
            {
                response.Success = false;
                response.Message = "Instagram Error: Unable to parse media creation response.";
                response.Raw = mediaResponseText;
                return response;
            }

            // STEP 2: Publish media
            var publishUrl =
                $"https://graph.facebook.com/v18.0/{_igUserId}/media_publish" +
                $"?creation_id={mediaResult.id}" +
                $"&access_token={_accessToken}";

            var publishResponse = await _http.PostAsync(publishUrl, null);
            var publishResponseText = await publishResponse.Content.ReadAsStringAsync();

            if (!publishResponse.IsSuccessStatusCode)
            {
                response.Success = false;
                response.Message = "Instagram Error (Publish Media): " + publishResponseText;
                response.Raw = publishResponseText;
                return response;
            }



            #region Facebook Post ::

            // FB photo post endpoint
            var fbUrl =
                $"https://graph.facebook.com/v18.0/{_igPageId}/photos" +
                $"?url={Uri.EscapeDataString(imageUrl)}" +
                $"&caption={Uri.EscapeDataString(caption)}" +
                $"&access_token={_accessToken}";
            var fbResponse = await _http.PostAsync(fbUrl, null);
            var fbRaw = await fbResponse.Content.ReadAsStringAsync();

            Console.WriteLine("FB RAW RESPONSE: " + fbRaw);

            if (!fbResponse.IsSuccessStatusCode)
            {
                response.Success = false;
                response.Message = "Facebook API Error: " + fbRaw;
                response.Raw = fbRaw;
                return response;
            }
            #endregion

            response.Success = true;
            response.Message = "Post published successfully";
            return response;
        }

    }
}
