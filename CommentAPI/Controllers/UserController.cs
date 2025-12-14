using Comment.Infrastructure.Services.User;
using Comment.Infrastructure.Services.User.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("Profile")]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromQuery] Guid id, CancellationToken cancellationToken)
        {
            var dto = new UserFindDto(id);
            var user = await _userService.GetByIdAsync(dto, cancellationToken);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] UserUpdateAvatarDTO dto, CancellationToken cancellationToken)
        {
            return await _userService.UpdateProfileAvatarAsync(dto, HttpContext, cancellationToken);
        }
    }
}

