using Microsoft.AspNetCore.Http;

namespace Comment.Core.Interfaces
{
    public interface IFileProvider
    {
        Task DeleteFileAsync(string url, CancellationToken cancellationToken);
        Task<bool> FileExists(string url);
        Task<string> GetFileUrlAsync(string url, CancellationToken cancellationToken);
        Task<string> SaveImageAsync(MemoryStream stream, string s3Key, string contentType, CancellationToken cancellationToken);
        Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken);

    }
}