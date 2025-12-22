namespace Comment.Infrastructure.Services.Comment.CreateComment.Request
{
    public record CommentCreateRequestDTO
    {
        public Guid? CallerId { get; set; }
        public string Content { get; set; }
        public Guid ThreadId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public string? FileKey { get; set; }
        public string? ContentType { get; set; }

        public CommentCreateRequestDTO(
            Guid? callerId,
            string content,
            Guid threadId,
            Guid? parentCommentId,
            string? filekey,
            string? contentType)
        {
            CallerId = callerId;
            Content = content;
            ThreadId = threadId;
            ParentCommentId = parentCommentId;
            FileKey = filekey;
            ContentType = contentType;
        }

        public CommentCreateRequestDTO() { }
    };
}
