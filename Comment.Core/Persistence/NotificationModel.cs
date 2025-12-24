namespace Comment.Core.Persistence
{
    public class NotificationModel 
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CreatorId { get; set; }
        public UserModel CreatorUser { get; set; }

        public Guid RecipientId { get; set; }

        public Guid CommentId { get; set; }
        public Guid ThreadId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
