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
        string txtAccessKeyId { get; }
        string txtSecretAccessKey { get; }
        string txtPublicUrl { get; }
        string txtStorageUrl { get; }
        string txtBucketName { get; }
    }
}