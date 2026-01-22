using SocialPostApi.Models.Responses;

namespace SocialPostApi.Services
{
    public interface IFacebookService
    {
        Task<SocialPostResult> PublishPost(string imageUrl, string message);
        Task<string> GetPublicImageUrl(string originalUrl);
    }
}
