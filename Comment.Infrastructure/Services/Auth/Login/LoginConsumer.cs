using Comment.Core.Data;
using Comment.Core.Interfaces;
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
        public async Task Consume(ConsumeContext<UserLoginRequest> context)
        {
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == context.Message.Email, context.CancellationToken);

            if (user == null)
            {
                await context.RespondAsync(new LoginNotFound("User not registered."));
                return;
            }
            if (!_passwordHasher.VerifyPassword(context.Message.Password, user.HashPassword))
            {
                await context.RespondAsync(new LoginUnauthorized());
                return;
            }

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync(context.CancellationToken);

            await context.RespondAsync(new LoginSuccesResponse(user.Id, user.UserName, user.Roles.ToList(), user));
        }
    }
}
