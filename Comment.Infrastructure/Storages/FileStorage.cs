using Comment.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Comment.Infrastructure.Storages
{
    public class FileStorage : IFileStorage
    {
        private readonly IConfiguration _configuration;

        public FileStorage(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string directory, string fileName)
        {
            var uploadPath = Path.Combine(_environment.ContentRootPath, directory);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public Task DeleteFileAsync(string filePath)
        {
            return Task.Run(() =>
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            });
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public string GetFileUrl(string directory, string filePath, Guid userId)
        {
            var fileName = Path.GetFileName(filePath);
            return $"/images/{userId}/{directory}/{fileName}";
        }

        public string GetFilePath(string directory, Guid userId)
        {
            return "test";
        }
    }
}