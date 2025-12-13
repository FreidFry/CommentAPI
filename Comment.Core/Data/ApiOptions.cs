using Comment.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Comment.Core.Data
{
    public class ApiOptions : IApiOptions
    {
        public string DbConnection { get; }
        public string S2Connection { get; }

        public ApiOptions(IConfiguration config)
        {
            DbConnection = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
            S2Connection = config.GetConnectionString("S2Connection") ?? throw new ArgumentNullException("S2Connection");
        }
    }
}
