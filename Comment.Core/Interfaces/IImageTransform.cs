using Microsoft.AspNetCore.Http;

namespace Comment.Core.Interfaces
{
    public interface IImageTransform
    {
        Task<string> CreateTumbnailAsync(IFormFile file, string dir, string name, CancellationToken cancellationToken);
        Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(IFormFile file, CancellationToken cancellationToken);
        Task<string> ProcessAndUploadGifAsync(IFormFile file, CancellationToken cancellationToken);
    }
}