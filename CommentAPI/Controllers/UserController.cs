using Comment.Infrastructure.Services.User;
using Comment.Infrastructure.Services.User.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("Profile")]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get user profile by id", Description = "Get user profile by id")]
        public async Task<IActionResult> GetById([FromRoute] Guid? id, CancellationToken cancellationToken)
        {
            return await _userService.GetByIdAsync(id, HttpContext, cancellationToken);
        }

        [HttpPut("avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] UserUpdateAvatarDTO dto, CancellationToken cancellationToken)
        {
            return await _userService.UpdateProfileAvatarAsync(dto, HttpContext, cancellationToken);
        }
    }
}

