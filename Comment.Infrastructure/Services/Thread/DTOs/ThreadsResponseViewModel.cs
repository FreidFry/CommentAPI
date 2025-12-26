using System.Text.Json.Serialization;

namespace Comment.Infrastructure.Services.Thread.DTOs
{
    /// <summary>
    /// View model representing a summary of a discussion thread for public display.
    /// Includes basic metadata and total engagement metrics.
    /// </summary>
    public class ThreadsResponseViewModel
    {
        /// <summary>Gets or sets the unique identifier for the thread.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        /// <summary>Gets or sets the title or headline of the thread.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets the main text or description of the thread.</summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>Gets or sets the date and time when the thread was originally published.</summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>Gets or sets the total number of comments associated with this thread.</summary>
        [JsonPropertyName("commentCount")]
        public int CommentCount { get; set; }

    }
}
