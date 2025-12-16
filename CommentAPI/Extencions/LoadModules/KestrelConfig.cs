namespace CommentAPI.Extencions.LoadModules
{
    public class KestrelConfig(IConfiguration configuration)
    {
        internal string certBase64 = configuration["Kestrel:CertBase64"] ?? throw new ArgumentNullException("CertBase64");
        internal string certPass = configuration["Kestrel:CertPassword"] ?? throw new ArgumentNullException("CertPassword");
        internal string httpDomen = configuration["Kestrel:HttpDomen"] ?? throw new ArgumentNullException("HttpDomen");
        internal string httpDomenSecure = configuration["Kestrel:httpDomenSecure"] ?? throw new ArgumentNullException("httpDomenSecure");
        internal int port = int.Parse(configuration["PORT"] ?? throw new ArgumentNullException("PORT"));
    }
}
