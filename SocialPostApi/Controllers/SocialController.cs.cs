using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SocialPostApi.Models;
using SocialPostApi.Models.Responses;
using SocialPostApi.Services;

namespace SocialPostApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialController : ControllerBase
    {
        private readonly IInstagramService _instagram;
        private readonly IFacebookService _facebook;
        private readonly ITwitterService _twitter;

        public SocialController(IInstagramService instagram, IFacebookService facebook, ITwitterService twitter)
        {
            _instagram = instagram;
            _facebook = facebook;
            _twitter = twitter;
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post([FromBody] SocialPostRequest request)
        {
            var results = new List<SocialPostResult>();

            if (request.Platforms.Contains("facebook"))
            {
                var fbResult = await _facebook.PublishPost(request.ImageUrl, request.Message);
                results.Add(fbResult);
            }

            if (request.Platforms.Contains("instagram_facebook"))
            {
                var instaResult = await _instagram.PublishPost(
                    request.ImageUrl!,
                    request.Message
                );
                results.Add(instaResult);
            }
            return Ok(results);
        }
        [EnableRateLimiting("twitter-limit")]
        [HttpPost("post/x")]
        public async Task<IActionResult> PostX([FromBody] SocialPostRequest request)
        {
            var results = new List<TwitterPostResponse>();

            if (request.Platforms.Contains("twitter"))
                results.Add(await _twitter.PostX(request.Message, request.ImageUrl));

            return Ok(results);
        }
    }
}
