using Comment.Core.Interfaces;
using Comment.Infrastructure.Services;
using Comment.Infrastructure.Services.Auth.Login;
using Comment.Infrastructure.Services.Auth.Logout;
using Comment.Infrastructure.Services.Auth.Register;
using Comment.Infrastructure.Services.Comment.CreateComment;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.DeleteComment;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread;
using Comment.Infrastructure.Services.Comment.GetReply;
using Comment.Infrastructure.Services.Comment.UpdateComment;
using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
using Comment.Infrastructure.Services.Thread.CreateThread;
using Comment.Infrastructure.Services.Thread.CreateThread.Request;
using Comment.Infrastructure.Services.Thread.DeleteThread;
using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread;
using Comment.Infrastructure.Services.Thread.GetThreadsTree;
using Comment.Infrastructure.Services.Thread.RestoreThread;
using Comment.Infrastructure.Services.Thread.UpdateThread;
using Comment.Infrastructure.Services.Thread.UpdateThread.Request;
using Comment.Infrastructure.Services.User.GetProfile;
using Comment.Infrastructure.Services.User.GetProfile.Request;
using Comment.Infrastructure.Services.User.Validators;
using Comment.Infrastructure.Utils;
using FluentValidation;
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
            services.AddScoped<IGetReplyHandler, GetReplyHandler>();

            services.AddScoped<ICreateThreadHandler, CreateThreadHandler>();
            services.AddScoped<IRestoreThreadHandler, RestoreThreadHandler>();
            services.AddScoped<IUpdateThreadHandler, UpdateThreadHandler>();
            services.AddScoped<IDeleteThreadHandler, DeleteThreadHandler>();
            services.AddScoped<IGetDetailedThreadHandler, GetDetailedThreadHandler>();
            services.AddScoped<IGetThreadTreeHandler, GetThreadTreeHandler>();

            services.AddScoped<IGetProfileHandler, GetProfileHandler>();

            #endregion

            #region Validators

            services.AddScoped<IValidator<CommentCreateRequest>, CommentCreateValidator>();
            services.AddScoped<IValidator<CommentUpdateRequest>, CommentUpdateValidator>();
            services.AddScoped<IValidator<DeleteCommentRequestDTO>, CommentDeleteValidator>();

            services.AddScoped<IValidator<ThreadCreateRequestDTO>, ThreadCreateValidator>();
            services.AddScoped<IValidator<DeleteThreadRequestDTO>, ThreadDeleteValidator>();
            services.AddScoped<IValidator<RestoreThreadRequestDTO>, RestoreThreadValidator>();
            services.AddScoped<IValidator<UpdateThreadRequestDTO>, ThreadUpdateValidator>();

            services.AddScoped<IValidator<ProfileGetRequestDTO>, GetProfileValidator>();

            #endregion

            #region Utils

            services.AddScoped<IPasswordHasher, PassworHasher>();
            services.AddSingleton<IFileProvider, FileProvider>();
            services.AddSingleton<IHtmlSanitize, HtmlSanitize>();

            #endregion
        }
    }
}
