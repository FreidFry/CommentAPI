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
        public static async Task<MemoryStream> CreateTumbnailAsync(IFormFile file, CancellationToken cancellationToken)
        {
            using var outputStream = new MemoryStream();

            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

            if (image.Width > 320 || image.Height > 240)
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(320, 240)
                }));
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder
            {
                Quality = 80
            }, cancellationToken);

            outputStream.Position = 0;
            return outputStream;
        }

        private static async Task<MemoryStream> CreateTumbnailGifAsync(IFormFile file, CancellationToken cancellationToken)
        {
            using var outputStream = new MemoryStream();

            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

            if (image.Width > 320 || image.Height > 240)
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(320, 240)
                }));

            await image.SaveAsGifAsync(outputStream, new GifEncoder
            {
                ColorTableMode = GifColorTableMode.Global,
                Quantizer = new SixLabors.ImageSharp.Processing.Processors.Quantization.OctreeQuantizer()
            }, cancellationToken);

            outputStream.Position = 0;
            return outputStream;
        }

        private static async Task<MemoryStream> GetOriginalImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var outputStream = new MemoryStream();
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

            await image.SaveAsJpegAsync(outputStream, new JpegEncoder
            {
                Quality = 70
            });

            outputStream.Position = 0;
            return outputStream;
        }

        private static async Task<MemoryStream> GetOriginalGifAsync(IFormFile file, CancellationToken cancellationToken)
        {
            using var outputStream = new MemoryStream();

            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);
            await image.SaveAsGifAsync(outputStream, new GifEncoder
            {
                ColorTableMode = GifColorTableMode.Global,
                Quantizer = new SixLabors.ImageSharp.Processing.Processors.Quantization.OctreeQuantizer()
            }, cancellationToken);

            outputStream.Position = 0;
            return outputStream;
        }

        private static string GetNewFileName(string name)
        {
            var newName = Uri.EscapeDataString(name);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return $"{timestamp}_{newName}";
        }

        public async Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var newName = GetNewFileName(file.FileName);
            var type = "image/jpeg";

            var tumbnail = await CreateTumbnailAsync(file, cancellationToken);
            var original = await GetOriginalImageAsync(file, cancellationToken);

            var tumbnailUrl = await _fileProvider.SaveImageAsync(tumbnail, newName, type, cancellationToken);
            var originalUlr = await _fileProvider.SaveImageAsync(original, newName, type, cancellationToken);

            File.Delete(file.FileName);
            return (originalUlr, tumbnailUrl);
        }

        public async Task<(string gifUrl, string gifTumbnailUrl)> ProcessAndUploadGifAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var newName = GetNewFileName(file.FileName);
            var type = "image/gif";

            var tumbnail = await CreateTumbnailGifAsync(file, cancellationToken);
            var originall = await GetOriginalGifAsync(file, cancellationToken);

            var tumbnailUrl = await _fileProvider.SaveImageAsync(tumbnail, newName, type, cancellationToken);
            var originallUrl = await _fileProvider.SaveImageAsync(tumbnail, newName, type, cancellationToken);

            File.Delete(file.FileName);
            return (originallUrl, tumbnailUrl);
        }
    }
}
