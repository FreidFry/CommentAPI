using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Auth.Register.Request;
using Comment.Infrastructure.Services.Auth.Register.Response;
using MassTransit;
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

        /// <summary>
/// Handles the user registration process by validating input, checking for existing users, 
/// hashing the password, and persisting the new user record.
/// </summary>
/// <param name="context">The consume context containing the <see cref="UserRegisterRequest"/>.</param>
/// <returns>
/// Sends a <see cref="RegisterSuccesResult"/> upon successful creation, 
/// or a <see cref="ConflictRegisterResponse"/> if validation fails or the user already exists.
/// </returns>
/// <remarks>
/// The registration flow includes:
/// <list type="number">
/// <item>Comparison of password and confirmation password.</item>
/// <item>Uniqueness check for the <c>UserName</c> in the database.</item>
/// <item>Password hashing using the injected <see cref="IPasswordHasher"/>.</item>
/// <item>Asynchronous persistence to the <see cref="AppDbContext"/>.</item>
/// </list>
/// </remarks>
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

            var newUser = new UserModel(context.Message.UserName, context.Message.Email, _passwordHasher.HashPassword(context.Message.Password), context.Message.HomePage);

            await _appDbContext.Users.AddAsync(newUser);
            await _appDbContext.SaveChangesAsync(context.CancellationToken);


            await context.RespondAsync(new RegisterSuccesResult(newUser.Id, newUser.UserName, newUser.Roles, newUser));
        }
    }
}
