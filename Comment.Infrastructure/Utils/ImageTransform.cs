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
            var outputStream = new MemoryStream();

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
            var outputStream = new MemoryStream();

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
            var outputStream = new MemoryStream();

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

        private static string GetNewFileName()
        {
            var guid = Guid.NewGuid().ToString()[..5];
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return $"{timestamp}_{guid}";
        }

        public async Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var newName = GetNewFileName();
            var type = "image/jpeg";

            using var tumbnail = await CreateTumbnailAsync(file, cancellationToken);
            using var original = await GetOriginalImageAsync(file, cancellationToken);

            var tumbnailUrl = await _fileProvider.SaveImageAsync(tumbnail, $"thumb_{newName}.jpg", type, cancellationToken);
            var originalUlr = await _fileProvider.SaveImageAsync(original, $"{newName}.jpg", type, cancellationToken);

            return (originalUlr, tumbnailUrl);
        }

        public async Task<(string gifUrl, string gifTumbnailUrl)> ProcessAndUploadGifAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var newName = GetNewFileName();
            var type = "image/gif";

            using var tumbnail = await CreateTumbnailGifAsync(file, cancellationToken);
            using var original = await GetOriginalGifAsync(file, cancellationToken);

            var tumbnailUrl = await _fileProvider.SaveImageAsync(tumbnail, $"thumb_{newName}.jpg", type, cancellationToken);
            var originalUrl = await _fileProvider.SaveImageAsync(original, $"{newName}.jpg", type, cancellationToken);

            return (originalUrl, tumbnailUrl);
        }
    }
}
