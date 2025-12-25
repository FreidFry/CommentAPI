namespace Comment.Core.Interfaces
{
    public interface IImageTransform
    {
        Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(Stream stream, CancellationToken cancellationToken);
        Task<(string gifUrl, string gifTumbnailUrl)> ProcessAndUploadGifAsync(Stream stream, CancellationToken cancellationToken);
    }
}