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
            
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(apiOptions.RedisConnect));

            services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new RedisConfiguration
            {
                ConnectionString = apiOptions.RedisConnect,
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = apiOptions.RedisConnect;
                options.InstanceName = apiOptions.RedisCapchaInstanceName;
            });

            return services;
        }

    }
}
