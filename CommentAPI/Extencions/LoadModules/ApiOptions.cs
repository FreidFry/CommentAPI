using Comment.Core.Interfaces;

namespace CommentAPI.Extencions.LoadModules
{
    public class ApiOptions : IApiOptions
    {
        public string DbConnection { get; }
        public string ImageAccessKeyId { get; }
        public string ImageSecretAccessKey {get;}
        public string ImagePublicUrl { get; }
        public string ImageStorageUrl { get; }
        public string ImageBucketName { get; }

        public string txtAccessKeyId { get; }
        public string txtSecretAccessKey { get; }
        public string txtPublicUrl { get; }
        public string txtStorageUrl { get; }
        public string txtBucketName { get; }

        public ApiOptions(IConfiguration config)
        {
            DbConnection = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
            ImageAccessKeyId =      config["S3:image:AccessKeyId"] ?? throw new ArgumentNullException("S3__Image__AccessKeyId");
            ImageSecretAccessKey =  config["S3:image:SecretAccessKey"] ?? throw new ArgumentNullException("S3__Image__SecretAccessKey");
            ImageStorageUrl =       config["S3:image:StorageUrl"] ?? throw new ArgumentNullException("S3__Image__StorageUrl");
            ImagePublicUrl =        config["S3:image:PublicUrl"] ?? throw new ArgumentNullException("S3__Image__PublicUrl");
            ImageBucketName =       config["S3:image:BucketName"] ?? throw new ArgumentNullException("S3__Image__BucketName");
            txtAccessKeyId =        config["S3:txt:AccessKeyId"] ?? throw new ArgumentNullException("S3__txt__AccessKeyId");
            txtSecretAccessKey =    config["S3:txt:SecretAccessKey"] ?? throw new ArgumentNullException("S3__txt__SecretAccessKey");
            txtStorageUrl =         config["S3:txt:StorageUrl"] ?? throw new ArgumentNullException("S3__txt__StorageUrl");
            txtPublicUrl =          config["S3:txt:PublicUrl"] ?? throw new ArgumentNullException("S3__txt__PublicUrl");
            txtBucketName =         config["S3:txt:BucketName"] ?? throw new ArgumentNullException("S3__txt__BucketName");
        }
    }
}
