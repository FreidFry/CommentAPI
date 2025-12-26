namespace Comment.Core.Interfaces
{
    /// <summary>
    /// Defines the global configuration settings for the API, including connection strings 
    /// and credentials for external storage, message brokers, and databases.
    /// </summary>
    public interface IApiOptions
    {
        /// <summary> Gets the connection string for the primary database. </summary>
        string DbConnection { get; }

        #region Image Storage (S3) Settings

        /// <summary> Gets the Access Key ID for the image storage provider. </summary>
        string ImageAccessKeyId { get; }

        /// <summary> Gets the Secret Access Key for the image storage provider. </summary>
        string ImageSecretAccessKey { get; }

        /// <summary> Gets the public-facing URL used to access uploaded images. </summary>
        string ImagePublicUrl { get; }

        /// <summary> Gets the name of the S3 bucket designated for images. </summary>
        string ImageBucketName { get; }

        /// <summary> Gets the service endpoint URL for the image storage API. </summary>
        string ImageStorageUrl { get; }

        #endregion

        #region Text Storage (S3) Settings

        /// <summary> Gets the Access Key ID for the text file storage provider. </summary>
        string TxtAccessKeyId { get; }

        /// <summary> Gets the Secret Access Key for the text file storage provider. </summary>
        string TxtSecretAccessKey { get; }

        /// <summary> Gets the public-facing URL used to access uploaded text files. </summary>
        string TxtPublicUrl { get; }

        /// <summary> Gets the service endpoint URL for the text storage API. </summary>
        string TxtStorageUrl { get; }

        /// <summary> Gets the name of the S3 bucket designated for text files. </summary>
        string TxtBucketName { get; }

        #endregion

        /// <summary> Gets the connection string for the RabbitMQ message broker. </summary>
        string RabbitMqConnect { get; }

        /// <summary> Gets the connection string for the Redis cache server. </summary>
        string RedisConnect { get; }

        /// <summary> Gets the instance name (prefix) used for general data in Redis. </summary>
        string RedisDataInstanceName { get; }

        /// <summary> Gets the instance name (prefix) used specifically for CAPTCHA data in Redis. </summary>
        string RedisCapchaInstanceName { get; }
    }
}