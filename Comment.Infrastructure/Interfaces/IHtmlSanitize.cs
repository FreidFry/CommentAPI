namespace Comment.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines methods for cleaning and validating HTML content to prevent XSS attacks and ensure safe rendering.
    /// </summary>
    public interface IHtmlSanitize
    {
        /// <summary>
        /// Sanitizes the provided HTML string by removing potentially dangerous tags, attributes, and scripts.
        /// </summary>
        /// <param name="html">The raw HTML string to be sanitized.</param>
        /// <returns>A safe, sanitized version of the HTML string.</returns>
        string Sanitize(string html);

        /// <summary>
        /// Determines whether the provided HTML string contains any visible text content after stripping all HTML tags.
        /// </summary>
        /// <param name="html">The HTML string to check.</param>
        /// <returns><c>true</c> if there is actual text content; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Useful for preventing the submission of empty comments that only contain tags (e.g., <c>&lt;p&gt;&lt;br&gt;&lt;/p&gt;</c>).
        /// </remarks>
        bool HasTextContent(string html);
    }
}