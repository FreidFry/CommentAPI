using Comment.Infrastructure.Services.Auth;
using Comment.Infrastructure.Services.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

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
        [SwaggerOperation(Summary = "Register a new user", Description = "Registers a new user with the provided details.")]
        [SwaggerResponse(200, "User registered successfully")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto, CancellationToken cancellationToken)
        {
            return await _authService.RegisterAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "User login", Description = "Logs in a user with the provided credentials.")]
        [SwaggerResponse(200, "User logged in successfully")]
        [SwaggerResponse(401, "Invalid password")]
        [SwaggerResponse(404, "Not found account")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto, CancellationToken cancellationToken)
        {
            return await _authService.LoginAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPost("logout")]
        [Authorize]
        [SwaggerOperation(Summary = "User logout", Description = "Logs out the currently authenticated user.")]
        public IActionResult Logout()
        {
            _authService.Logout(HttpContext);
            return Ok(new { Message = "Logout successful" });
        }

        [HttpGet("init")]
        [Authorize]
        [SwaggerOperation(Summary = "Initialize user session", Description = "")]
        public async Task<IActionResult> InitAsync()
        {
            return await _authService.Init(HttpContext);
        }
    }
}

