namespace SocialPostApi.Models.Responses
{
    public class TwitterPostResponse
    {
        public bool Success { get; set; }
        public string? TweetId { get; set; }
        public string? MediaId { get; set; }
        public string? Text { get; set; }
        public string Raw { get; set; }
    }
}
