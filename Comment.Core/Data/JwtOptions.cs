using Comment.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Comment.Core.Data
{
    public class JwtOptions : IJwtOptions
    {
        public string SecretKey { get; }
        public string Issuer { get; }
        public string Audience { get; }
        public double ExpiresDays { get; }


        public JwtOptions(IConfiguration config)
        {
            SecretKey = config["SECRET_KEY"] ?? throw new ArgumentNullException("SecretKey");
            Issuer = config["ISSUER"] ?? throw new ArgumentNullException("Issuer");
            Audience = config["AUDIENCE"] ?? throw new ArgumentNullException("Audience");
            ExpiresDays = double.Parse(config["EXPIRES_DAYS"] ?? throw new ArgumentNullException("ExpiresDays"));

            if (SecretKey.Length < 32)
                throw new ArgumentException("SecretKey must be at least 32 characters long");
        }
    }
}
