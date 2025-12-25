using Serilog;

namespace CommentAPI.Extencions.LoadModules
{
    public static class AddSerilogExtension
    {
        public static WebApplicationBuilder AddSirilogLogger(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateBootstrapLogger();

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());

            return builder;
        }
    }
}
