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
        public async Task<string> CreateTumbnailAsync(IFormFile file, string path, string dir, string name, CancellationToken cancellationToken)
        {
            var ext = ".jpg";
            var outputPath = Path.Combine(dir, $"{name}_tumb{ext}");
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

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

        private async Task<string> ResizeGifAsync(IFormFile file, string path, string dir, string name, CancellationToken cancellationToken)
        {
            var ext = ".gif";
            var outputPath = Path.Combine(dir, $"{name}_thumb{ext}");
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

        private async Task<string> ChangeExtensionAsync(IFormFile file, string path, string dir, string name, CancellationToken cancellationToken)
        {
            if (file.ContentType != "image/jpeg")
            {
                var ext = ".jpg";
                var outputPath = Path.Combine(dir, $"{name}{ext}");
                using var stream = file.OpenReadStream();
                using var image = await Image.LoadAsync(stream, cancellationToken);

                image.Save($"{outputPath}", new JpegEncoder
                {
                    Quality = 70
                });
                return outputPath;
            }
            return path;
        }

        public async Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var filename = file.FileName;
            var dir = Path.GetDirectoryName(filename)!;
            var name = Path.GetFileNameWithoutExtension(filename);

            var tumbnail = await CreateTumbnailAsync(file, filename, dir, name, cancellationToken);
            var original = await ChangeExtensionAsync(file, filename, dir, name, cancellationToken);

            var tumbnailurl = await _fileProvider.SaveImageAsync(tumbnail, cancellationToken);
            var resizedlurl = await _fileProvider.SaveImageAsync(original, cancellationToken);
            File.Delete(Path.Combine(dir,filename));
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
