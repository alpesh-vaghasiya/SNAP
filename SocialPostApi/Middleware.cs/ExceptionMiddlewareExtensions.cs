namespace SocialPostApi.Middleware
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ApiExceptionMiddleware>();
        }
    }
}
