using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Infrastructure.Services;
using Comment.Infrastructure.Services.Auth;
using Comment.Infrastructure.Services.Comment;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.Validators;
using Comment.Infrastructure.Services.Thread;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.Validators;
using Comment.Infrastructure.Services.User;
using Comment.Infrastructure.Services.User.DTOs.Request;
using Comment.Infrastructure.Services.User.Validators;
using Comment.Infrastructure.Storages;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace CommentAPI.Extencions.LoadModules
{
    public static class AddDiExtencions
    {
        public static void AddDipedencyInjections(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, PassworHasher>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddSingleton<ITokenStorage, TokenStorage>();
            services.AddSingleton<IFileProvider, FileProvider>();

            services.AddSingleton<IJwtOptions>(sp =>
            {
                var jwtOptions = sp.GetRequiredService<IConfiguration>();
                return new JwtOptions(jwtOptions);
            });
            services.AddSingleton<IApiOptions>(sp =>
            {
                var apiOptions = sp.GetRequiredService<IConfiguration>();
                return new ApiOptions(apiOptions);
            });

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IThreadService, ThreadService>();
            services.AddScoped<IImageTransform, ImageTransform>();

            // Validators
            services.AddScoped<IValidator<CommentCreateDTO>, CommentCreateValidator>();
            services.AddScoped<IValidator<CommentUpdateDTO>, CommentUpdateValidator>();
            services.AddScoped<IValidator<CommentFindDTO>, CommentFindValidator>();
            services.AddScoped<IValidator<ThreadCreateDTO>, ThreadCreateValidator>();
            services.AddScoped<IValidator<ThreadUpdateDTO>, ThreadUpdateValidator>();
            services.AddScoped<IValidator<ThreadFindDTO>, ThreadFindValidator>();
            services.AddScoped<IValidator<UserCreateDTO>, UserCreateValidator>();
            services.AddScoped<IValidator<UserUpdateAvatarDTO>, UserUpdateAvatarValidator>();
        }
    }
}
