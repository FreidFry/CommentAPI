using Microsoft.Extensions.Configuration;

namespace Comment.Core.Interfaces
{
    public class ApiOptions(IConfiguration config)
    {
        public string DbConnection { get; } = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
        public string S2Connection { get; } = config.GetConnectionString("S2Connection") ?? throw new ArgumentNullException("S2Connection");
        public int ExpiresDays { get; } = int.Parse(config["EXPIRES_DAYS"] ?? throw new ArgumentNullException("EXPIRES_DAYS"));
    }
}
