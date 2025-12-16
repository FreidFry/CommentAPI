using Comment.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Comment.Core.Data
{
    public class ApiOptions : IApiOptions
    {
        public string DbConnection { get; }
        public string AccessKeyId { get; }
        public string SecretAccessKey {get;}
        public string ServiceUrl { get; }
        public string StorageUrl { get; }
        public string BucketName { get; }

        public ApiOptions(IConfiguration config)
        {
            DbConnection = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
            AccessKeyId = config["S3:AccessKeyId"] ?? throw new ArgumentNullException("S3_AccessKeyId");
            SecretAccessKey = config["S3:SecretAccessKey"] ?? throw new ArgumentNullException("S3_SecretAccessKey");
            StorageUrl = config["S3:StorageUrl"] ?? throw new ArgumentNullException("S3_StorageUrl");
            ServiceUrl = config["S3:ServiceUrl"] ?? throw new ArgumentNullException("S3_ServiceUrl");
            BucketName = config["S3:BucketName"] ?? throw new ArgumentNullException("S3_BucketName");
        }
    }
}
