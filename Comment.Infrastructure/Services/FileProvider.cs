using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Comment.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Comment.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="IFileProvider"/> using Amazon S3-compatible storage.
    /// Handles file and image uploads with automated logging and performance tracking.
    /// </summary>
    public class FileProvider : IFileProvider
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IApiOptions _apiOptions;
        private readonly ILogger<FileProvider> _logger;

        public FileProvider(IApiOptions apiOptions, ILogger<FileProvider> logger)
        {
            _apiOptions = apiOptions;

            var endpointUrl = apiOptions.ImageStorageUrl;

            var config = new AmazonS3Config
            {
                ServiceURL = endpointUrl,
                ForcePathStyle = true
            };

            var credentials = new BasicAWSCredentials(apiOptions.ImageAccessKeyId, apiOptions.ImageSecretAccessKey);
            _s3Client = new AmazonS3Client(credentials, config);
            _logger = logger;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Uploads an image to the designated image bucket and returns the public access URL.
        /// </remarks>
        public async Task<string> SaveImageAsync(MemoryStream stream, string s3Key, string contentType, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var normalizedS3Key = Uri.EscapeDataString(s3Key);
            var key = $"uploads/{normalizedS3Key}";
            try
            {
                _logger.LogDebug("S3: Uploading image {Key} ({Size} KB)", key, stream.Length / 1024);

                var transferUtility = new TransferUtility(_s3Client);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _apiOptions.ImageBucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType,
#if DEBUG
                    CannedACL = S3CannedACL.PublicRead,
#endif
#if RELEASE
                    DisablePayloadSigning = true,
#endif
                };
                await transferUtility.UploadAsync(uploadRequest, cancellationToken);
                sw.Stop();

                var fileUrl = $"{_apiOptions.ImagePublicUrl.TrimEnd('/')}/{key}";
                _logger.LogInformation("S3: Image uploaded successfully. Key: {Key}, Time: {Elapsed}ms", key, sw.ElapsedMilliseconds);
                return fileUrl;
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "S3: Provider error during image upload. Key: {Key}, Status: {Status}", key, s3Ex.StatusCode);
                throw new Exception($"Failed to upload file to S3: {s3Ex.Message}", s3Ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S3: Unexpected error during image upload. Key: {Key}", key);
                throw;
            }
        }

        private static string GetNewFileName() =>
    $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid().ToString()[..5]}";

        /// <inheritdoc />
        /// <remarks>
        /// Uploads a generic file (defaulting to plain text) to the designated text bucket.
        /// </remarks>
        public async Task<string> SaveFileAsync(Stream stream, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var key = $"uploads/{GetNewFileName()}";

            try
            {
                _logger.LogDebug("S3: Uploading file {Key} ({Size} KB)", key, stream.Length / 1024);

                var transferUtility = new TransferUtility(_s3Client);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    BucketName = _apiOptions.TxtBucketName,
                    Key = key,
#if DEBUG
                    CannedACL = S3CannedACL.PublicRead,
#endif
#if RELEASE
                    DisablePayloadSigning = true,
#endif
                    ContentType = "text/plain; charset=utf-8"
                };

                await transferUtility.UploadAsync(uploadRequest, cancellationToken);
                sw.Stop();
                var fileUrl = $"{_apiOptions.TxtPublicUrl.TrimEnd('/')}/{key}";

                _logger.LogInformation("S3: File uploaded. Key: {Key}, Time: {Elapsed}ms", key, sw.ElapsedMilliseconds);

                return fileUrl;
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "S3: Provider error during file upload. Key: {Key}, Status: {Status}", key, s3Ex.StatusCode);
                throw new Exception($"Failed to upload file to S3: {s3Ex.Message}", s3Ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S3: Unexpected error during file upload. Key: {Key}", key);
                throw;
            }
        }
    }
}