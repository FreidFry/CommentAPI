using Comment.Core.Interfaces;

namespace CommentAPI.Extencions.LoadModules
{
    public class JwtOptions : IJwtOptions
    {
        public string SecretKey { get; }
        public string Issuer { get; }
        public string Audience { get; }
        public double ExpiresDays { get; }


        public JwtOptions(IConfiguration config)
        {
            SecretKey = config["JWT:SECRET_KEY"] ?? throw new ArgumentNullException("SecretKey");
            Issuer = config["JWT:ISSUER"] ?? throw new ArgumentNullException("Issuer");
            Audience = config["JWT:AUDIENCE"] ?? throw new ArgumentNullException("Audience");
            ExpiresDays = double.Parse(config["JWT:EXPIRES_DAYS"] ?? throw new ArgumentNullException("ExpiresDays"));

            if (SecretKey.Length < 32)
                throw new ArgumentException("SecretKey must be at least 32 characters long");
        }
    }
}
