using Comment.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request
{
    /// <summary>
    /// Represents a request to retrieve a paginated list of comments for a specific thread.
    /// Supports cursor-based navigation, sorting, and direct focusing on a specific comment.
    /// </summary>
    public class CommentsByThreadRequest
    {
        /// <summary>
        /// Gets or sets the field name to sort by. 
        /// Default is "createat".
        /// </summary>
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        public string SortBy { get; set; } = "createat";

        /// <summary>
        /// Internal helper that maps the <see cref="SortBy"/> string to a typed <see cref="SortByEnum"/>.
        /// Ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public SortByEnum SortByEnum
        {
            get => Enum.TryParse<SortByEnum>(SortBy, true, out var result) ? result : SortByEnum.CreateAt;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the sort order is ascending.
        /// Default is false (descending).
        /// </summary>
        public bool IsAscending { get; set; } = false;

        /// <summary>
        /// The cursor for pagination. Typically represents the encoded ID or timestamp 
        /// of the last item from the previous page.
        /// </summary>
        public string? After { get; set; }

        /// <summary>
        /// The maximum number of comments to return in a single request. 
        /// Default is 25.
        /// </summary>
        public int Limit { get; set; } = 25;

        /// <summary>
        /// Optional identifier of a specific comment to highlight or jump to within the thread.
        /// </summary>
        public Guid? FocusCommentId { get; set; }
    }
}
