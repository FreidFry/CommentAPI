using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Infrastructure.Services.Auth.DTOs;
using Comment.Infrastructure.Utils;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;

namespace Comment.Infrastructure.Services.Auth.Login
{
    public class LoginConsumer : IConsumer<UserLoginRequest>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IJwtOptions _envOptions;

        public LoginConsumer(AppDbContext appDbContext, IPasswordHasher passwordHasher, IJwtProvider jwtProvider, IJwtOptions envOptions)
        {
            _appDbContext = appDbContext;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _envOptions = envOptions;
        }
        public async Task Consume(ConsumeContext<UserLoginRequest> context)
        {
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == context.Message.Email);

            if (user == null)
            {
                await context.RespondAsync(new NotFoundResult());
                return;
            }
            if (!_passwordHasher.VerifyPassword(context.Message.Password, user.HashPassword))
            {
                await context.RespondAsync(new UnauthorizedResult());
                return;
            }

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync();

            await context.RespondAsync(new LoginResponse(user.Id, user.UserName, user.Roles.ToList(), user));
        }
    }
}
