using Comment.Infrastructure.Services.Comment.DTOs.Response;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response
{

    /// <summary>
    /// Represents a paginated response containing a list of comments and metadata for the next page.
    /// </summary>
    public record CommentsListResponse
    {

        /// <summary>
        /// Gets or sets the collection of comment view models for the current page.
        /// </summary>
        public List<CommentViewModel>? Items { get; set; }

        /// <summary>
        /// Gets or sets the opaque string used as a cursor to fetch the next page of results.
        /// Returns null if there are no further items.
        /// </summary>
        public string? NextCursor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there are more comments available to be fetched.
        /// </summary>
        public bool HasMore { get; set; }
    }
}
