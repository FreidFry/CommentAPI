
namespace Comment.Infrastructure.Services.Thread.DTOs.Response
{
    public class ThreadResponseDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public string OwnerUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int CommentCount { get; set; }
    }
}

