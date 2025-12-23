using Comment.Core.Interfaces;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace CommentAPI.Extencions.LoadModules
{
    public static class AddRedisCacheExtension
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IApiOptions apiOptions)
        {
            var connectionString = apiOptions.RedisConnect;

            if (connectionString.StartsWith("redis://"))
            {
                var uriString = connectionString.Replace("redis://", "");

                var atIndex = uriString.LastIndexOf('@');

                if (atIndex != -1)
                {
                    var auth = uriString.Substring(0, atIndex).Split(':');
                    var host = uriString.Substring(atIndex + 1);

                    var user = auth[0];
                    var pass = auth.Length > 1 ? auth[1] : "";

                    connectionString = $"{host},user={user},password={pass},abortConnect=false";
                }
                else
                {
                    connectionString = $"{uriString},abortConnect=false";
                }
            }
            else if (!connectionString.Contains("abortConnect"))
            {
                connectionString += ",abortConnect=false";
            }
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(connectionString));

            services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new RedisConfiguration
            {
                ConnectionString = connectionString,
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
                options.InstanceName = apiOptions.RedisCapchaInstanceName;
            });
            var signalRBuilder = services.AddSignalR();
            signalRBuilder.AddStackExchangeRedis(connectionString, options =>
            {
                options.Configuration.ChannelPrefix = apiOptions.RedisDataInstanceName;
            });

            return services;
        }

    }
}
