using Microsoft.AspNetCore.Http;

namespace Comment.Core.Interfaces
{
    public interface IImageTransform
    {
        Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(IFormFile file, CancellationToken cancellationToken);
        Task<(string gifUrl, string gifTumbnailUrl)> ProcessAndUploadGifAsync(IFormFile file, CancellationToken cancellationToken);
    }
}