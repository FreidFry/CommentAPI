using Microsoft.AspNetCore.Http;

namespace Comment.Core.Interfaces
{
    public interface IFileStorage
    {
        Task DeleteFileAsync(string filePath);
        bool FileExists(string filePath);
        string GetFilePath(string directory, Guid userId);
        string GetFileUrl(string directory, string filePath, Guid userId);
        Task<string> SaveFileAsync(IFormFile file, string directory, string fileName);
    }
}