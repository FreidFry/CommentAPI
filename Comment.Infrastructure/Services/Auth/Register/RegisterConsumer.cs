using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Auth.Register.Request;
using Comment.Infrastructure.Services.Auth.Register.Response;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Auth.Register
{
    public class RegisterConsumer : IConsumer<UserRegisterRequest>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterConsumer(AppDbContext appDbContext, IPasswordHasher passwordHasher)
        {
            _appDbContext = appDbContext;
            _passwordHasher = passwordHasher;
        }

        async Task IConsumer<UserRegisterRequest>.Consume(ConsumeContext<UserRegisterRequest> context)
        {
            if (!context.Message.Password.Equals(context.Message.ConfirmPassword))
            {
                await context.RespondAsync(new ConflictRegisterResponse("Passwords do not match"));
                return;
            }
            var existingUser = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.UserName == context.Message.UserName);

            if (existingUser != null)
            {
                await context.RespondAsync(new ConflictRegisterResponse("User already exists"));
                return;
            }

            var newUser = new UserModel(context.Message.UserName, context.Message.Email, _passwordHasher.HashPassword(context.Message.Password));

            await _appDbContext.Users.AddAsync(newUser);
            await _appDbContext.SaveChangesAsync(context.CancellationToken);


            await context.RespondAsync(new OkResult());
        }
    }
}
