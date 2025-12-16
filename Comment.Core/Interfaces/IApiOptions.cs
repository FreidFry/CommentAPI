namespace Comment.Core.Interfaces
{
    public interface IApiOptions
    {
        string DbConnection { get; }
        string AccessKeyId { get; }
        string SecretAccessKey { get; }
        string ServiceUrl { get; }
        string BucketName { get; }
        string StorageUrl { get; }
    }
}