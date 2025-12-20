using Comment.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace Comment.Infrastructure.Services.Comment.DTOs.Request
{
    public record CommentsByThreadDTO
    {

        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        public string SortBy { get; } = "createAt";
        [JsonIgnore]
        public SortByEnum SortByEnum
        {
            get => Enum.TryParse<SortByEnum>(SortBy, true, out var result) ? result : SortByEnum.CreateAt;
        }
        public bool IsAscending { get; }
        public string? After { get; } = null;
        public int Limit { get; } = 25;
    }
}
