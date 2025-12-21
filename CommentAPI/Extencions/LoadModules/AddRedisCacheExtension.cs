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
            services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new RedisConfiguration()
            {
                ConnectionString = apiOptions.RedisConnect
            });
            return services;
        }

    }
}
