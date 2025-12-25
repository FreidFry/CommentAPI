using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Comment.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Comment.Infrastructure.Services
{
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

        /// <summary>
        /// Stores a file from a local path in Cloudflare R2 (S3)
        /// </summary>
        /// <param name="path">Local path to the file</param>
        /// <param name="cancellationToken">Сancellation Token</param>
        /// <returns>Public URL of the uploaded file</returns>
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
                    DisablePayloadSigning = true
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
                    DisablePayloadSigning = true,
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