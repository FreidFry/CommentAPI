using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Comment.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Services
{
    public class FileProvider : IFileProvider
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IApiOptions _apiOptions;

        public FileProvider(IApiOptions apiOptions)
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
        }

        /// <summary>
        /// Stores a file from a local path in Cloudflare R2 (S3)
        /// </summary>
        /// <param name="path">Local path to the file</param>
        /// <param name="cancellationToken">Сancellation Token</param>
        /// <returns>Public URL of the uploaded file</returns>
        public async Task<string> SaveImageAsync(MemoryStream stream, string s3Key, string contentType, CancellationToken cancellationToken)
        {
            try
            {
                var normalizedS3Key = Uri.EscapeDataString(s3Key);
                var key = $"uploads/{normalizedS3Key}";

                var s = new TransferUtility(_s3Client);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _apiOptions.ImageBucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType,
                    DisablePayloadSigning = true
                };
                await s.UploadAsync(uploadRequest, cancellationToken);

                var fileUrl = $"{_apiOptions.ImagePublicUrl.TrimEnd('/')}/{key}";

                return fileUrl;
            }
            catch (AmazonS3Exception s3Ex)
            {
                throw new Exception($"Failed to upload file to S3: {s3Ex.Message}", s3Ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var normalizedS3Key = Uri.EscapeDataString(file.FileName);

            var key = $"uploads/{timestamp}_{normalizedS3Key}.txt";

            try
            {
                using var stream = file.OpenReadStream();

                var s = new TransferUtility(_s3Client);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    BucketName = _apiOptions.TxtBucketName,
                    Key = key,
                    DisablePayloadSigning = true,
                    ContentType = "text/plain; charset=utf-8"
                };
                await s.UploadAsync(uploadRequest, cancellationToken);

                var fileUrl = $"{_apiOptions.TxtPublicUrl.TrimEnd('/')}/{key}";
                return fileUrl;
            }
            catch (Exception ex)
            {
                throw;

            }
        }
        private static string GetNewFileName() =>
    $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid().ToString()[..5]}";


        public async Task<string> SaveFileAsync(Stream stream, CancellationToken cancellationToken)
        {

            var key = $"uploads/{GetNewFileName()}";

            try
            {
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

                var fileUrl = $"{_apiOptions.TxtPublicUrl.TrimEnd('/')}/{key}";
                return fileUrl;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}