using Microsoft.AspNetCore.Http;

namespace Comment.Core.Interfaces
{
    public interface IFileProvider
    {
        Task DeleteFileAsync(string url, CancellationToken cancellationToken);
        Task<bool> FileExists(string url);
        Task<string> GetFileUrlAsync(string url, CancellationToken cancellationToken);
        Task<string> SaveFileAsync(string path, CancellationToken cancellationToken);
    }
}