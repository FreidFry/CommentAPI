namespace CommentAPI.Extencions.LoadModules
{
    public class KestrelConfig(IConfiguration configuration)
    {
        internal string certBase64 = configuration["Kestrel:CertBase64"] ?? throw new ArgumentNullException("CertBase64");
        internal string certPass = configuration["Kestrel:CertPassword"] ?? throw new ArgumentNullException("CertPassword");
    }
}
