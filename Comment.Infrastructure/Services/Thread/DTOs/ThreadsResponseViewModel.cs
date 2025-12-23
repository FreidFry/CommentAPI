using System.Text.Json.Serialization;

namespace Comment.Infrastructure.Services.Thread.DTOs
{
    public class ThreadsResponseViewModel
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("title")]public string Title { get; set; } = string.Empty;
        [JsonPropertyName("content")]public string Content { get; set; } = string.Empty;
        [JsonPropertyName("createdAt")]public DateTime CreatedAt { get; set; }
        [JsonPropertyName("commentCount")] public int CommentCount { get; set; }

    }
}
