using Microsoft.AspNetCore.Http;

namespace Comment.Core.Interfaces
{
    public interface IFileProvider
    {
        Task<string> SaveImageAsync(MemoryStream stream, string s3Key, string contentType, CancellationToken cancellationToken);
        Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken);
        Task<string> SaveFileAsync(Stream stream, CancellationToken cancellationToken);
    }
}