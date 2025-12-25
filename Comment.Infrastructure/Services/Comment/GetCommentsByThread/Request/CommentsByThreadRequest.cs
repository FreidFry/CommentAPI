using Comment.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request
{
    public class CommentsByThreadRequest
    {
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        public string SortBy { get; set; } = "createat";
        [JsonIgnore]
        public SortByEnum SortByEnum
        {
            get => Enum.TryParse<SortByEnum>(SortBy, true, out var result) ? result : SortByEnum.CreateAt;
        }
        public bool IsAscending { get; set; } = false;
        public string? After { get; set; }
        public int Limit { get; set; } = 25;
        public Guid? FocusCommentId { get; set; }
    }
}
