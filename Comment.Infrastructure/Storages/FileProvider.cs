using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Comment.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace Comment.Infrastructure.Storages
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
        public async Task<string> SaveImageAsync(string path, CancellationToken cancellationToken)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            try
            {
                var fileName = Path.GetFileName(path);
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var guid = Guid.NewGuid().ToString("N")[..8];
                var key = $"uploads/{timestamp}_{guid}_{fileName}";

                var s = new TransferUtility(_s3Client);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _apiOptions.ImageBucketName,
                    Key = key,
                    FilePath = path,
                    ContentType = GetContentType(fileName),
                    DisablePayloadSigning = true
                };
                await s.UploadAsync(uploadRequest, cancellationToken);

                var fileUrl = $"{_apiOptions.ImagePublicUrl.TrimEnd('/')}/{key}";

                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                }

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

        /// <summary>
        /// Gets the public URL of a file from S3
        /// </summary>
        /// <param name="url">URL or file key in S3</param>
        /// <param name="cancellationToken">Сancellation Token</param>
        /// <returns>Публичный URL файла</returns>
        public async Task<string> GetFileUrlAsync(string url, CancellationToken cancellationToken)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return url;
            }

            var key = url.TrimStart('/');
            return await Task.FromResult($"{_apiOptions.ImagePublicUrl.TrimEnd('/')}/{key}");
        }

        /// <summary>
        /// Checks for the existence of a file in S3
        /// </summary>
        /// <param name="url">URL or file key in S3</param>
        /// <returns>True if the file exists, otherwise False</returns>
        public async Task<bool> FileExists(string url)
        {
            try
            {
                var key = ExtractKeyFromUrl(url);

                var request = new GetObjectMetadataRequest
                {
                    BucketName = _apiOptions.ImageBucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a file from S3
        /// </summary>
        /// <param name="url">URL or file key in S3</param>
        /// <param name="cancellationToken">Сancellation Token</param>
        public async Task DeleteFileAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                var key = ExtractKeyFromUrl(url);

                var request = new DeleteObjectRequest
                {
                    BucketName = _apiOptions.ImageBucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request, cancellationToken);

            }
            catch (AmazonS3Exception s3Ex)
            {
                throw new Exception($"Failed to delete file from S3: {s3Ex.Message}", s3Ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static string ExtractKeyFromUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return uri.AbsolutePath.TrimStart('/');
            }
            return url.TrimStart('/');
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "text/plain"

            };
        }

        public async Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken)
        {

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var key = $"uploads/{timestamp}_{guid}.txt";
            try
            {
                using var stream = file.OpenReadStream();

                var s = new TransferUtility(_s3Client);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    BucketName = _apiOptions.txtBucketName,
                    Key = key,
                    DisablePayloadSigning = true,
                    ContentType = "text/plain"
                };
                await s.UploadAsync(uploadRequest, cancellationToken);

                var fileUrl = $"{_apiOptions.txtPublicUrl.TrimEnd('/')}/{key}";
                return fileUrl;
            }
            catch (Exception ex)
            {
                throw;

            }
        }
    }
}