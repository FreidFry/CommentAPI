namespace Comment.Infrastructure.CommonDTOs
{
    public record NotificationViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime CreateAt { get; set; }
        public string CreatorName { get; set; }
        public string CreatorAvatarUrl { get; set; }
        public Guid ThreadId { get; set; }
        public Guid CommentId { get; set; }
        public bool IsRead { get; set; }

        public NotificationViewModel()
        {
            
        }

        public NotificationViewModel(Guid Id, string Title, string Message, string Type, DateTime CreateAt, string CreatorName, string CreatorAvatarUrl, Guid ThreadId, Guid CommentId, bool IsRead)
        {
            this.Id = Id;
            this.Title = Title;
            this.Message = Message;
            this.Type = Type;
            this.CreateAt = CreateAt;
            this.CreatorName = CreatorName;
            this.CreatorAvatarUrl = CreatorAvatarUrl;
            this.ThreadId = ThreadId;
            this.CommentId = CommentId;
            this.IsRead = IsRead;
        }

    }
}
