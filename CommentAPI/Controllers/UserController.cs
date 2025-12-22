using Comment.Infrastructure.Services.User.GetProfile;
using Comment.Infrastructure.Services.User.GetProfile.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("Profile")]
    [Authorize]
    public class UserController(IGetProfileHandler getProfileHandler) : ControllerBase
    {
        private readonly IGetProfileHandler _getProfileHandler = getProfileHandler;

        [HttpGet("{Id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get user profile by id", Description = "Get user profile by id")]
        public async Task<IActionResult> GetById([FromRoute] ProfileGetRequest request, CancellationToken cancellationToken)
        {
            return await _getProfileHandler.Handle(request, HttpContext, cancellationToken);
        }

        //[HttpPut("avatar")]
        //public async Task<IActionResult> UpdateAvatar([FromBody] UserUpdateAvatarDTO dto, CancellationToken cancellationToken)
        //{
        //    return await _userService.UpdateProfileAvatarAsync(dto, HttpContext, cancellationToken);
        //}
    }
}

