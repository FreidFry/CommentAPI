using System.Security.Cryptography.X509Certificates;

namespace CommentAPI.Extencions.LoadModules
{
    public static class PortThrowExtension
    {
        public static IServiceCollection AddPortConfiguration(this IServiceCollection services,
            IWebHostBuilder hostBuilder, KestrelConfig kestrel)
        {
            //var certBytes = Convert.FromBase64String(kestrel.certBase64);
            //var cert = new X509Certificate2(certBytes!, kestrel.certPass);


            hostBuilder.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(kestrel.port);
                //options.ListenAnyIP(8081, listenOptions =>
                //{
                //    listenOptions.UseHttps(cert);
                //});
            });
            //services.AddHttpsRedirection(builder =>
            //{
            //    builder.HttpsPort = 443;
            //});

            return services;
        }
    }
}
