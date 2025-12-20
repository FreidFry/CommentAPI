
namespace Comment.Infrastructure.Services.Thread.DTOs.Response
{
    public record DetailedThreadResponse
    {
        public DetailedThreadResponse() { }
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Context { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int CommentCount { get; set; }
    }
}

