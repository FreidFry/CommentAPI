namespace Comment.Core.Interfaces
{
    /// <summary>
    /// Responsible for image processing and further delegation for uploading.
    /// </summary>
    public interface IImageTransform
    {

        /// <summary>
        /// Receives a <see cref="Stream"/> to process the image and upload it via an <see cref="IFileProvider"/> implementation.
        /// </summary>
        /// <param name="stream">The image data stream.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the operation, containing the processed image and thumbnail URLs.</returns>
        Task<(string imageUrl, string imageTumbnailUrl)> ProcessAndUploadImageAsync(Stream stream, CancellationToken cancellationToken);

        /// <summary>
        /// Receives a <see cref="Stream"/> to process an animated image and upload it via an <see cref="IFileProvider"/> implementation.
        /// </summary>
        /// <param name="stream">The animated image data stream.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the operation, containing the processed GIF and thumbnail URLs.</returns>
        Task<(string gifUrl, string gifTumbnailUrl)> ProcessAndUploadGifAsync(Stream stream, CancellationToken cancellationToken);
    }
}