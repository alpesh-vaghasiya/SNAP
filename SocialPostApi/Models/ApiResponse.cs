namespace SocialPostApi.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
        public ApiError? Error { get; set; }
    }

    public class ApiError
    {
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string? Details { get; set; }
    }
}
