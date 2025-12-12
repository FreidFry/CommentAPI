using Microsoft.Extensions.Configuration;

namespace Comment.Core.Interfaces
{
    public class JwtOptions(IConfiguration config)
    {
        public string SecretKey { get; } = config["SECRET_KEY"] ?? throw new ArgumentNullException("SecretKey");
        public string Issuer { get; } = config["ISSUER"] ?? throw new ArgumentNullException("Issuer");
        public string Audience { get; } = config["AUDIENCE"] ?? throw new ArgumentNullException("Audience");
        public double ExpiresDays { get; } = double.Parse(config["EXPIRES_DAYS"] ?? throw new ArgumentNullException("ExpiresDays"));
    }
}
