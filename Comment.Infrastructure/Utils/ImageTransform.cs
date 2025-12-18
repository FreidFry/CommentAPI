using Comment.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
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
        public async Task<string> CreateTumbnailAsync(IFormFile file, string dir, string newName, CancellationToken cancellationToken)
        {
            var ext = ".jpg";
            var outputPath = Path.Combine(dir, $"{newName}_tumb{ext}");
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

            if (image.Width > 320 || image.Height > 240)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(320, 240)
                }));
            }
            image.Save($"{outputPath}", new JpegEncoder
            {
                Quality = 80
            });

            return outputPath;
        }

        private async Task<string> ResizeGifAsync(IFormFile file, string path, string dir, string newName, CancellationToken cancellationToken)
        {
            var ext = ".gif";
            var outputPath = Path.Combine(dir, $"{newName}_thumb{ext}");
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);

            if (image.Width > 320 || image.Height > 240)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(320, 240)
                }));
                image.Save($"{outputPath}", new GifEncoder
                {
                    ColorTableMode = GifColorTableMode.Global,
                    Quantizer = new SixLabors.ImageSharp.Processing.Processors.Quantization.OctreeQuantizer()
                });

                return outputPath;
            }
            return path;
        }

        private async Task<string> ChangeExtensionAsync(IFormFile file, string dir, string newName, CancellationToken cancellationToken)
        {
            var ext = ".jpg";
            var outputPath = Path.Combine(dir, $"{newName}{ext}");
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

            image.Save($"{outputPath}", new JpegEncoder
            {
                Quality = 70
            });
            return outputPath;
        }

        private string GetNewName()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            return Path.Combine(timestamp, guid);
        }

        public async Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var filename = file.FileName;
            var dir = Path.GetDirectoryName(filename)!;
            var newName = GetNewName();

            var tumbnail = await CreateTumbnailAsync(file, dir, newName, cancellationToken);
            var original = await ChangeExtensionAsync(file, dir, newName, cancellationToken);

            var tumbnailurl = await _fileProvider.SaveImageAsync(tumbnail, cancellationToken);
            var resizedlurl = await _fileProvider.SaveImageAsync(original, cancellationToken);
            File.Delete(Path.Combine(dir, filename));
            return (resizedlurl, tumbnailurl);
        }

        public async Task<string> ProcessAndUploadGifAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var filename = file.FileName;
            var dir = Path.GetDirectoryName(filename)!;
            var name = Path.GetFileNameWithoutExtension(filename);

            var resized = await ResizeGifAsync(file, filename, dir, name, cancellationToken);

            var resizedlurl = await _fileProvider.SaveImageAsync(resized, cancellationToken);
            return resizedlurl;
        }
    }
}
