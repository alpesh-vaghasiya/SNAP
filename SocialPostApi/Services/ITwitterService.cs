using SocialPostApi.Models.Responses;

namespace SocialPostApi.Services
{
    public interface ITwitterService
    {
        Task<TwitterPostResponse> PostX(string message, string imageUrl);
    }
}
