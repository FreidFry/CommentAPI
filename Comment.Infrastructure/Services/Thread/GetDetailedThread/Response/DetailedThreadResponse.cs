using System.Text.Json.Serialization;

namespace Comment.Infrastructure.Services.Thread.GetDetailedThread.Response
{
    public class DetailedThreadResponse
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
        [JsonPropertyName("context")] public string Context { get; set; } = string.Empty;
        [JsonPropertyName("ownerId")] public Guid OwnerId { get; set; }
        [JsonPropertyName("ownerUserName")] public string OwnerUserName { get; set; } = string.Empty;
        [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonPropertyName("lastUpdatedAt")] public DateTime? LastUpdatedAt { get; set; }
        [JsonPropertyName("commentCount")] public int CommentCount { get; set; }
    }
}

