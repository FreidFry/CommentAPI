using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Auth.DTOs;
using Comment.Infrastructure.Services.Auth.Login.Response;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Auth.Login
{
    public class LoginConsumer : IConsumer<UserLoginRequest>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPasswordHasher _passwordHasher;

        public LoginConsumer(AppDbContext appDbContext, IPasswordHasher passwordHasher)
        {
            _appDbContext = appDbContext;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Handles the user login process by validating credentials and returning an authentication response.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="UserLoginRequest"/> message.</param>
        /// <returns>
        /// Sends a <see cref="LoginSuccesResponse"/> on success, 
        /// or a <see cref="StatusCodeResponse"/> with 404 (Not Found) or 403 (Forbidden) on failure.
        /// </returns>
        /// <remarks>
        /// The method performs the following steps:
        /// <list type="bullet">
        /// <item>Checks if the user exists in the database by email.</item>
        /// <item>Verifies the provided password against the stored hash.</item>
        /// <item>Updates the user entity (e.g., last login timestamp) and saves changes.</item>
        /// <item>Responds to the requester via MassTransit's Request-Response pattern.</item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<UserLoginRequest> context)
        {
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == context.Message.Email, context.CancellationToken);

            if (user == null)
            {
                await context.RespondAsync(new StatusCodeResponse("User not registered.", 404));
                return;
            }
            if (!_passwordHasher.VerifyPassword(context.Message.Password, user.HashPassword))
            {
                await context.RespondAsync(new StatusCodeResponse("Forbind.", 403));
                return;
            }

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync(context.CancellationToken);

            await context.RespondAsync(new LoginSuccesResponse(user.Id, user.UserName, user.Roles.ToList(), user));
        }
    }
}
