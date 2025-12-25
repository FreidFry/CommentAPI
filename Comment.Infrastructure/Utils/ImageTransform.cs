using Comment.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System.Diagnostics;

namespace Comment.Infrastructure.Utils
{
    public class ImageTransform : IImageTransform
    {
        private readonly IFileProvider _fileProvider;
        private readonly ILogger<ImageTransform> _logger;
        public ImageTransform(IFileProvider fileProvider, ILogger<ImageTransform> logger)
        {
            _fileProvider = fileProvider;
            _logger = logger;
        }

        public async Task<MemoryStream> CreateTumbnailAsync(Stream stream, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var outputStream = new MemoryStream();
            try
            {
                using var image = await Image.LoadAsync(stream, cancellationToken);
                _logger.LogDebug("Thumbnail: Image loaded. Original size: {Width}x{Height}", image.Width, image.Height);

                if (image.Width > 320 || image.Height > 240)
                {

                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(320, 240)
                    }));
                    _logger.LogTrace("Thumbnail: Image resized to max 320x240.");
                }
                await image.SaveAsJpegAsync(outputStream, new JpegEncoder
                {
                    Quality = 80
                }, cancellationToken);

                outputStream.Position = 0;
                stream.Position = 0;
                sw.Stop();
                _logger.LogInformation("Thumbnail created: {Width}x{Height}, Result size: {Size} KB, Time: {Elapsed}ms",
                    image.Width, image.Height, outputStream.Length / 1024, sw.ElapsedMilliseconds);
                return outputStream;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Thumbnail processing was cancelled (Timeout or User disconnected).");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Thumbnail: Failed to process image.");
                throw;
            }
        }

        private async Task<MemoryStream> CreateTumbnailGifAsync(Stream stream, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var outputStream = new MemoryStream();
            try
            {
                using var image = await Image.LoadAsync(stream, cancellationToken);

                int frameCount = image.Frames.Count;
                _logger.LogDebug("GIF Thumbnail: Loaded. Frames: {Frames}, Size: {W}x{H}",
                    frameCount, image.Width, image.Height);
                if (frameCount > 100)
                    _logger.LogWarning("Heavy GIF detected: {Frames} frames. This may cause high CPU usage.", frameCount);


                if (image.Width > 320 || image.Height > 240)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(320, 240)
                    }));
                    _logger.LogTrace("GIF Thumbnail: Resized all {Frames} frames.", frameCount);
                }

                await image.SaveAsGifAsync(outputStream, new GifEncoder
                {
                    ColorTableMode = GifColorTableMode.Global,
                    Quantizer = new OctreeQuantizer()
                }, cancellationToken);

                outputStream.Position = 0;
                stream.Position = 0;
                sw.Stop();
                _logger.LogInformation("GIF Thumbnail created. Frames: {Frames}, Result: {Size} KB, Time: {Elapsed}ms",
                    frameCount, outputStream.Length / 1024, sw.ElapsedMilliseconds);
                return outputStream;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GIF processing was cancelled (Timeout or User disconnected).");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during GIF thumbnail creation.");
                throw;
            }
        }

        private async Task<MemoryStream> GetOriginalImageAsync(Stream stream, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var outputStream = new MemoryStream();

            try
            {
                long inputLength = stream.CanSeek ? stream.Length : 0;
                using var image = await Image.LoadAsync(stream, cancellationToken);
                _logger.LogDebug("Original: Processing {Width}x{Height} image. Input size: {Size} KB",
                    image.Width, image.Height, inputLength / 1024);
                await image.SaveAsJpegAsync(outputStream, new JpegEncoder
                {
                    Quality = 70
                });

                outputStream.Position = 0;
                stream.Position = 0;
                sw.Stop();
                _logger.LogInformation("Original saved: {Width}x{Height}, Output: {Size} KB, Saved: {Saved} KB, Time: {Elapsed}ms",
                    image.Width,
                    image.Height,
                    outputStream.Length / 1024,
                    (inputLength - outputStream.Length) / 1024,
                    sw.ElapsedMilliseconds);
                return outputStream;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Original processing was cancelled (Timeout or User disconnected).");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Original: Error during image re-encoding to JPEG 70%");
                throw;
            }
        }

        private async Task<MemoryStream> GetOriginalGifAsync(Stream stream, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var outputStream = new MemoryStream();

            try
            {
                long inputLength = stream.CanSeek ? stream.Length : 0;

                using var image = await Image.LoadAsync(stream, cancellationToken);
                int frameCount = image.Frames.Count;

                _logger.LogDebug("Original GIF: Loaded {W}x{H} with {Frames} frames. Input: {Size} KB",
                    image.Width, image.Height, frameCount, inputLength / 1024);
                await image.SaveAsGifAsync(outputStream, new GifEncoder
                {
                    ColorTableMode = GifColorTableMode.Global,
                    Quantizer = new OctreeQuantizer()
                }, cancellationToken);

                outputStream.Position = 0;
                stream.Position = 0;
                sw.Stop();
                _logger.LogInformation("Original GIF processed: {Frames} frames, Output: {Size} KB, Time: {Elapsed}ms",
                    frameCount,
                    outputStream.Length / 1024,
                    sw.ElapsedMilliseconds);

                if (sw.ElapsedMilliseconds > 3000)
                    _logger.LogWarning("Performance Alert: GIF processing took too long ({Elapsed}ms) for {Frames} frames",
                        sw.ElapsedMilliseconds, frameCount);

                return outputStream;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Original GIF: Processing cancelled by timeout.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Original GIF: Critical failure during encoding.");
                throw;
            }
        }

        private static string GetNewFileName() =>
            $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid().ToString()[..5]}";

        public async Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(Stream stream, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var newName = GetNewFileName();
            var type = "image/jpeg";

            _logger.LogInformation("Starting Image Upload Pipeline for {Name}. Input stream length: {Len} bytes",
                newName, stream.CanSeek ? stream.Length : "unknown");

            try
            {
                var thumbTask = CreateTumbnailAsync(stream, ct);
                var origTask = GetOriginalImageAsync(stream, ct);

                await Task.WhenAll(thumbTask, origTask);

                using var thumbImg = await thumbTask;
                using var origImg = await origTask;

                _logger.LogDebug("Images processed in memory. Starting upload to storage...");
                var uploadSw = Stopwatch.StartNew();
                var thumbUrl = await _fileProvider.SaveImageAsync(thumbImg, $"thumb_{newName}.jpg", type, ct);
                var originalUrl = await _fileProvider.SaveImageAsync(origImg, $"{newName}.jpg", type, ct);
                uploadSw.Stop();
                sw.Stop();
                _logger.LogInformation("Pipeline completed for {Name}. Total: {Total}ms (Upload: {Upload}ms). URLs: {Orig}, {Thumb}",
                    newName, sw.ElapsedMilliseconds, uploadSw.ElapsedMilliseconds, originalUrl, thumbUrl);
                return (originalUrl, thumbUrl);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Image Pipeline: Processing cancelled by timeout.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image Pipeline failed for {Name} after {Elapsed}ms", newName, sw.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<(string gifUrl, string gifTumbnailUrl)> ProcessAndUploadGifAsync(Stream stream, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var newName = GetNewFileName();
            var type = "image/gif";

            _logger.LogInformation("GIF Pipeline started: {Name}", newName);
            try
            {
                var thumbTask = CreateTumbnailGifAsync(stream, ct);
                var origTask = GetOriginalGifAsync(stream, ct);

                await Task.WhenAll(thumbTask, origTask);

                using var tumbnail = await thumbTask;
                using var original = await origTask;

                _logger.LogDebug("GIF processing finished (Time: {Elapsed}ms). Starting upload...", sw.ElapsedMilliseconds);

                var uploadSw = Stopwatch.StartNew();

                var tumbnailUrl = await _fileProvider.SaveImageAsync(tumbnail, $"thumb_{newName}.gif", type, ct);
                var originalUrl = await _fileProvider.SaveImageAsync(original, $"{newName}.gif", type, ct);

                uploadSw.Stop();
                sw.Stop();

                _logger.LogInformation("GIF Pipeline completed: {Name}. Total: {Total}ms, Upload part: {Upload}ms",
                    newName, sw.ElapsedMilliseconds, uploadSw.ElapsedMilliseconds);

                return (originalUrl, tumbnailUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GIF Pipeline failed for {Name} after {Elapsed}ms", newName, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
