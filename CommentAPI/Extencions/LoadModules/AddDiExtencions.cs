using Comment.Infrastructure.Services.Auth;
using Comment.Infrastructure.Services.User;

namespace CommentAPI.Extencions.LoadModules
{
    public static class AddDiExtencions
    {
        public static void AddDipedencyInjections(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
        }
    }
}
