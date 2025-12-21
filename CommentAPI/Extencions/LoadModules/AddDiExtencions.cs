using Comment.Core.Interfaces;
using Comment.Infrastructure.Services;
using Comment.Infrastructure.Services.Auth.Login;
using Comment.Infrastructure.Services.Auth.Logout;
using Comment.Infrastructure.Services.Auth.Register;
using Comment.Infrastructure.Services.Comment;
using Comment.Infrastructure.Services.Comment.CreateComment;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.DeleteComment;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread;
using Comment.Infrastructure.Services.Comment.UpdateComment;
using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
using Comment.Infrastructure.Services.Comment.Validators;
using Comment.Infrastructure.Services.Thread;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.Validators;
using Comment.Infrastructure.Services.User;
using Comment.Infrastructure.Services.User.DTOs.Request;
using Comment.Infrastructure.Services.User.Validators;
using Comment.Infrastructure.Utils;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace CommentAPI.Extencions.LoadModules
{
    public static class AddDiExtencions
    {
        public static void AddDipedencyInjections(this IServiceCollection services)
        {
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

            #region Services

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IThreadService, ThreadService>();
            services.AddScoped<IImageTransform, ImageTransform>();
            services.AddScoped<IJwtProvider, JwtProvider>();

            #endregion

            #region Handlers

            services.AddScoped<ILoginHandler, LoginHandler>();
            services.AddScoped<IRegisterHandler, RegisterHandler>();
            services.AddScoped<ILogoutHandler, LogoutHandler>();
            services.AddScoped<ICreateCommentHandler, CreateCommentHandler>();
            services.AddScoped<IGetCommentsByThreadHandler, GetCommentsByThreadHandler>();
            services.AddScoped<IUpdateCommentHandler, UpdateCommentHandler>();
            services.AddScoped<IDeleteCommentHandler, DeleteCommentHandler>();

            #endregion

            #region Validators

            services.AddScoped<IValidator<CommentCreateRequest>, CommentCreateValidator>();
            services.AddScoped<IValidator<CommentUpdateRequest>, CommentUpdateValidator>();
            services.AddScoped<IValidator<CommentFindDTO>, CommentFindValidator>();
            services.AddScoped<IValidator<DeleteCommentRequestDTO>, CommentDeleteValidator>();
            services.AddScoped<IValidator<ThreadCreateDTO>, ThreadCreateValidator>();
            services.AddScoped<IValidator<ThreadUpdateDTO>, ThreadUpdateValidator>();
            services.AddScoped<IValidator<ThreadFindDTO>, ThreadFindValidator>();
            services.AddScoped<IValidator<UserCreateDTO>, UserCreateValidator>();
            services.AddScoped<IValidator<UserUpdateAvatarDTO>, UserUpdateAvatarValidator>();

            #endregion

            #region Utils

            services.AddScoped<IPasswordHasher, PassworHasher>();
            services.AddSingleton<IFileProvider, FileProvider>();
            services.AddSingleton<IHtmlSanitize, HtmlSanitize>();
            #endregion
        }
    }
}
