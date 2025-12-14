using Comment.Infrastructure.Services.Auth;
using Comment.Infrastructure.Services.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto, CancellationToken cancellationToken)
        {
            return await _authService.RegisterAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto, CancellationToken cancellationToken)
        {
            return await _authService.Login(dto, HttpContext, cancellationToken);
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            _authService.Logout(HttpContext);
            return Ok(new { Message = "Logout successful" });
        }

        [HttpGet("init")]
        [Authorize]
        public IActionResult Init()
        {
            var userId = _authService.Init(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Ok(new { UserId = userId });
        }
    }
}

