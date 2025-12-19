using Comment.Core.Interfaces;

namespace CommentAPI.Extencions.LoadModules
{
    public class ApiOptions : IApiOptions
    {

        #region data base

        public string DbConnection { get; }
        public string DbName { get; }
        public string DbUser { get; }
        public string DbPassword { get; }
        public string DbServer { get; }
        public string DbPort { get; }

        #endregion

        public string RabbitMqConnect { get; }

        #region S3

        #region img

        public string ImageAccessKeyId { get; }
        public string ImageSecretAccessKey { get; }
        public string ImagePublicUrl { get; }
        public string ImageStorageUrl { get; }
        public string ImageBucketName { get; }

        #endregion

        #region .txt

        public string TxtAccessKeyId { get; }
        public string TxtSecretAccessKey { get; }
        public string TxtPublicUrl { get; }
        public string TxtStorageUrl { get; }
        public string TxtBucketName { get; }

        #endregion

        #endregion
        public ApiOptions(IConfiguration config)
        {
            DbName = config["DB:DbName"] ?? throw new ArgumentNullException("DB:DbName");
            DbUser = config["DB:User"] ?? throw new ArgumentNullException("DB:User");
            DbPassword = config["DB:Password"] ?? throw new ArgumentNullException("DB:Password");
            DbServer = config["DB:Server"] ?? throw new ArgumentNullException("DB:Server");
            DbPort = config["DB:Port"] ?? "5432";

            RabbitMqConnect = config["RabbitMq:ConnectString"] ?? throw new ArgumentNullException("RabbitMq__ConnectString");

            DbConnection = $"Server={DbServer};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword}";

            ImageAccessKeyId = config["S3:image:AccessKeyId"] ?? throw new ArgumentNullException("S3__Image__AccessKeyId");
            ImageSecretAccessKey = config["S3:image:SecretAccessKey"] ?? throw new ArgumentNullException("S3__Image__SecretAccessKey");
            ImageStorageUrl = config["S3:image:StorageUrl"] ?? throw new ArgumentNullException("S3__Image__StorageUrl");
            ImagePublicUrl = config["S3:image:PublicUrl"] ?? throw new ArgumentNullException("S3__Image__PublicUrl");
            ImageBucketName = config["S3:image:BucketName"] ?? throw new ArgumentNullException("S3__Image__BucketName");
            TxtAccessKeyId = config["S3:txt:AccessKeyId"] ?? throw new ArgumentNullException("S3__txt__AccessKeyId");
            TxtSecretAccessKey = config["S3:txt:SecretAccessKey"] ?? throw new ArgumentNullException("S3__txt__SecretAccessKey");
            TxtStorageUrl = config["S3:txt:StorageUrl"] ?? throw new ArgumentNullException("S3__txt__StorageUrl");
            TxtPublicUrl = config["S3:txt:PublicUrl"] ?? throw new ArgumentNullException("S3__txt__PublicUrl");
            TxtBucketName = config["S3:txt:BucketName"] ?? throw new ArgumentNullException("S3__txt__BucketName");
        }
    }
}
