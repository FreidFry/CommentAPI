namespace Comment.Core.Interfaces
{
    public interface IApiOptions
    {
        string DbConnection { get; }
        string ImageAccessKeyId { get; }
        string ImageSecretAccessKey { get; }
        string ImagePublicUrl { get; }
        string ImageBucketName { get; }
        string ImageStorageUrl { get; }
        string TxtAccessKeyId { get; }
        string TxtSecretAccessKey { get; }
        string TxtPublicUrl { get; }
        string TxtStorageUrl { get; }
        string TxtBucketName { get; }
        string RabbitMqConnect { get; }
        string RedisConnect { get; }
        string RedisDataInstanceName { get; }
        string RedisCapchaInstanceName { get; }
        string DbSSLMode { get; }
        string DbTrustServCert { get; }
    }
}