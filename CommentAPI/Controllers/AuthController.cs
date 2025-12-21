using Comment.Infrastructure.Services.Auth;
using Comment.Infrastructure.Services.Auth.DTOs;
using Comment.Infrastructure.Services.Auth.Login;
using Comment.Infrastructure.Services.Auth.Logout;
using Comment.Infrastructure.Services.Auth.Register;
using Comment.Infrastructure.Services.Auth.Register.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginHandler _loginHandler;
        private readonly IRegisterHandler _registerHandler;
        private readonly ILogoutHandler _logoutHandler;

        public AuthController(ILoginHandler loginHandler, IRegisterHandler registerHandler, ILogoutHandler logoutHandler)
        {
            _loginHandler = loginHandler;
            _registerHandler = registerHandler;
            _logoutHandler = logoutHandler;
        }
        
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Register a new user", Description = "Registers a new user with the provided details.")]
        [SwaggerResponse(200, "User registered successfully")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest dto, CancellationToken cancellationToken)
        {
            return await _registerHandler.RegisterHandleAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "User login", Description = "Logs in a user with the provided credentials.")]
        [SwaggerResponse(200, "User logged in successfully")]
        [SwaggerResponse(401, "Invalid password")]
        [SwaggerResponse(404, "Not found account")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest dto, CancellationToken cancellationToken)
        {
            return await _loginHandler.HandleLoginAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPost("logout")]
        [Authorize]
        [SwaggerOperation(Summary = "User logout", Description = "Logs out the currently authenticated user.")]
        public IActionResult Logout()
        {
            _logoutHandler.Logout(HttpContext);
            return Ok(new { Message = "Logout successful" });
        }
    }
}

