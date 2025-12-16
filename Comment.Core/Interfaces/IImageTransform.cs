using Microsoft.AspNetCore.Http;

namespace Comment.Core.Interfaces
{
    public interface IImageTransform
    {
        Task<string> CreateTumbnailAsync(IFormFile file, string path, string dir, string name, CancellationToken cancellationToken);
        Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(IFormFile file, CancellationToken cancellationToken);
        Task<string> ResizeImageAsync(IFormFile file, string path, string dir, string name, CancellationToken cancellationToken);
    }
}