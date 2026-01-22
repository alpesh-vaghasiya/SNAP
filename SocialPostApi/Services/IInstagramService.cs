using SocialPostApi.Models.Responses;

namespace SocialPostApi.Services
{
    public interface IInstagramService
    {
        Task<SocialPostResult> PublishPost(string imageUrl, string caption);
    }
}