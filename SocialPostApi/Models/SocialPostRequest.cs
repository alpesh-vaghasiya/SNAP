namespace SocialPostApi.Models
{
    public class SocialPostRequest
    {
        public required string Message { get; set; }
        public string? ImageUrl { get; set; }
        public required List<string> Platforms { get; set; }
    }
    public class MediaResponse
    {
        public string id { get; set; } = string.Empty;
    }
    public class FbLongLivedTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
    public class PostTweetDto
    {
        public string Message { get; set; }
        public string ImageUrl { get; set; }
    }
}

