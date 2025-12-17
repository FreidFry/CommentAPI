using Comment.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Comment.Infrastructure.Utils
{
    public class ImageTransform : IImageTransform
    {
        private readonly IFileProvider _fileProvider;
        public ImageTransform(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }
        public async Task<string> ResizeImageAsync(IFormFile file, string path, string dir, string name, CancellationToken cancellationToken)
        {
            var ext = ".jpg";
            var outputPath = Path.Combine(dir, $"{name}_320x240{ext}");
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);

            if (image.Width > 320 || image.Height > 240)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(320, 240)
                }));
                image.Save($"{outputPath}", new JpegEncoder
                {
                    Quality = 80
                });

                return outputPath;
            }
            return path;
        }

        public async Task<string> CreateTumbnailAsync(IFormFile file, string path, string dir, string name, CancellationToken cancellationToken)
        {
            var ext = ".jpg";
            var outputPath = Path.Combine(dir, $"{name}_thumb{ext}");
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(100, 100)
            }));
            image.Save($"{outputPath}", new JpegEncoder
            {
                Quality = 80
            });

            return outputPath;
        }

        public async Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var filename = file.FileName;
            var dir = Path.GetDirectoryName(filename)!;
            var name = Path.GetFileNameWithoutExtension(filename);

            var tumbnail = await CreateTumbnailAsync(file, filename, dir, name, cancellationToken);
            var resized = await ResizeImageAsync(file, filename, dir, name, cancellationToken);

            var tumbnailurl = await _fileProvider.SaveFileAsync(tumbnail, cancellationToken);
            var resizedlurl = await _fileProvider.SaveFileAsync(resized, cancellationToken);
            return (resizedlurl, tumbnailurl);
        }
    }
}
