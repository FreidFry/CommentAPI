using Comment.Infrastructure.Maps;

namespace CommentAPI.Extencions.LoadModules
{
    public static class AddAutoMapperExtension
    {
        public static IServiceCollection AddAutoMapperModule(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(
                //typeof(UserMapProfile).Assembly), 
                typeof(UserMapProfile), typeof(CommentMapProfile), typeof(ThreadsMapProfile)));
            return services;
        }
    }
}
