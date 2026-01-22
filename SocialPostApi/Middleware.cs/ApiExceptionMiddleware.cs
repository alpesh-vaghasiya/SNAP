using System.Net;
using System.Text.Json;

namespace SocialPostApi.Middleware
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error Occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = HttpStatusCode.InternalServerError;

            // If TwitterException / custom exception → we can inspect
            if (ex.GetType().Name == "TwitterException")
                statusCode = HttpStatusCode.Unauthorized;

            var response = new
            {
                success = false,
                error = ex.Message,
                type = ex.GetType().Name,
                path = context.Request.Path,
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
