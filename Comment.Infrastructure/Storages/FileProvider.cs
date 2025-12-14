using Comment.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Storages
{
    public class FileProvider : IFileProvider
    {
        public Task DeleteFileAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string filePath)
        {
            throw new NotImplementedException();
        }

        public string GetFileUrl(string directory, string filePath, Guid userId)
        {
            throw new NotImplementedException();
        }

        public string GetFilePath(string directory, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveFileAsync(IFormFile file, string directory, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}