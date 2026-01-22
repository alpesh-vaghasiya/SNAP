using Microsoft.AspNetCore.RateLimiting;
using SocialPostApi.Filters;
using SocialPostApi.Middleware;
using SocialPostApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IInstagramService, InstagramService>();
builder.Services.AddHttpClient<IFacebookService, FacebookService>();
builder.Services.AddScoped<ITwitterService, TwitterService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("twitter-limit", limiter =>
    {
        limiter.PermitLimit = 1; // Maximum 1 calls
        limiter.Window = TimeSpan.FromMinutes(1); // per 1 minute
        limiter.QueueLimit = 0; // no queue
    });
});

var app = builder.Build();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseApiExceptionHandling();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
