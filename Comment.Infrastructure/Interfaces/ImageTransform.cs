using Comment.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Comment.Infrastructure.Interfaces
{
    public class ImageTransform : IImageTransform
    {
        private readonly IFileProvider _fileProvider;
        public ImageTransform(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public static async Task<MemoryStream> CreateTumbnailAsync(Stream stream, CancellationToken cancellationToken)
        {
            var outputStream = new MemoryStream();

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
            stream.Position = 0;
            return outputStream;
        }

        private static async Task<MemoryStream> CreateTumbnailGifAsync(Stream stream, CancellationToken cancellationToken)
        {
            var outputStream = new MemoryStream();
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
                Quantizer = new OctreeQuantizer()
            }, cancellationToken);

            outputStream.Position = 0;
            stream.Position = 0;
            return outputStream;
        }

        private static async Task<MemoryStream> GetOriginalImageAsync(Stream stream, CancellationToken cancellationToken)
        {
            var outputStream = new MemoryStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

            await image.SaveAsJpegAsync(outputStream, new JpegEncoder
            {
                Quality = 70
            });

            outputStream.Position = 0;
            stream.Position = 0;
            return outputStream;
        }

        private static async Task<MemoryStream> GetOriginalGifAsync(Stream stream, CancellationToken cancellationToken)
        {
            var outputStream = new MemoryStream();

            using var image = await Image.LoadAsync(stream, cancellationToken);
            await image.SaveAsGifAsync(outputStream, new GifEncoder
            {
                ColorTableMode = GifColorTableMode.Global,
                Quantizer = new OctreeQuantizer()
            }, cancellationToken);

            outputStream.Position = 0;
            stream.Position = 0;
            return outputStream;
        }

        private static string GetNewFileName() =>
            $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid().ToString()[..5]}";

        public async Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(Stream stream, CancellationToken ct)
        {
            var newName = GetNewFileName();
            var type = "image/jpeg";

            using var thumbImg = await CreateTumbnailAsync(stream, ct);
            using var origImg = await GetOriginalImageAsync(stream, ct);

            var thumbUrl = await _fileProvider.SaveImageAsync(thumbImg, $"thumb_{newName}.jpg", type, ct);
            var originalUrl = await _fileProvider.SaveImageAsync(origImg, $"{newName}.jpg", type, ct);

            return (originalUrl, thumbUrl);
        }

        public async Task<(string gifUrl, string gifTumbnailUrl)> ProcessAndUploadGifAsync(Stream stream, CancellationToken ct)
        {
            var newName = GetNewFileName();
            var type = "image/gif";

            using var tumbnail = await CreateTumbnailGifAsync(stream, ct);
            using var original = await GetOriginalGifAsync(stream, ct);

            var tumbnailUrl = await _fileProvider.SaveImageAsync(tumbnail, $"thumb_{newName}.gif", type, ct);
            var originalUrl = await _fileProvider.SaveImageAsync(original, $"{newName}.gif", type, ct);

            return (originalUrl, tumbnailUrl);
        }
    }
}
