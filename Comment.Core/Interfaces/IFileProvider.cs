namespace Comment.Core.Interfaces
{
    /// <summary>
    /// Defines a contract for file storage operations, providing methods to save images and documents 
    /// to a persistent storage provider (e.g., S3, Local Storage).
    /// </summary>
    public interface IFileProvider
    {
        /// <summary>
        /// Asynchronously saves an image to the storage provider.
        /// </summary>
        /// <param name="stream">The memory stream containing the image data.</param>
        /// <param name="s3Key">The unique key or file name under which the image will be stored.</param>
        /// <param name="contentType">The MIME type of the image (e.g., "image/jpeg", "image/png").</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the public URL of the uploaded image.</returns>
        Task<string> SaveImageAsync(MemoryStream stream, string s3Key, string contentType, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously saves a generic file to the storage provider.
        /// </summary>
        /// <param name="stream">The data stream of the file to be saved.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the public URL of the uploaded file.</returns>
        Task<string> SaveFileAsync(Stream stream, CancellationToken cancellationToken);
    }
}