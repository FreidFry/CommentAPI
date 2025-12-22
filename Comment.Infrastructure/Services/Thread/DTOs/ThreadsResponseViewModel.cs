namespace Comment.Infrastructure.Services.Thread.DTOs
{
    public class ThreadsResponseViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int CommentCount { get; set; }

    }
}
