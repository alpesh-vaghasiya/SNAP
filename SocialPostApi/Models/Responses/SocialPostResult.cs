namespace SocialPostApi.Models.Responses
{
    public class SocialPostResult
    {
        public string Platform { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? PostId { get; set; }
        public object? Raw { get; set; }
    }
}
