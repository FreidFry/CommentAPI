using Comment.Core.Interfaces;

namespace CommentAPI.Extencions.LoadModules
{
    public static class AddSignalRExtension
    {
        public static IServiceCollection SetupSignalR(this IServiceCollection services, IApiOptions apiOptions)
        {
            var runMode = Environment.GetEnvironmentVariable("RUN_MODE") ?? "All";


            var signalRBuilder = services.AddSignalR();
            if (runMode == "API" || runMode == "All")
            {
                signalRBuilder.AddJsonProtocol(o =>
                {
                    o.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            }
            signalRBuilder.AddStackExchangeRedis(apiOptions.RedisConnect, options =>
            {
                options.Configuration.ChannelPrefix = apiOptions.RedisDataInstanceName;
            });
            return services;
        }
    }
}
