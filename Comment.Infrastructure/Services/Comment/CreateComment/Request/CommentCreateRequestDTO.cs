namespace Comment.Infrastructure.Services.Comment.CreateComment.Request
{
    /// <summary>
    /// Represents the request payload for creating a new comment.
    /// Supports nested replies, captcha verification, and optional file attachments.
    /// </summary>
    /// <param name="Content">The text body of the comment.</param>
    /// <param name="ThreadId">The unique identifier of the discussion thread.</param>
    /// <param name="CaptchaId">The unique ID used to identify the captcha session.</param>
    /// <param name="CaptchaValue">The user's input for captcha verification.</param>
    /// <param name="ParentCommentId">The ID of the parent comment (if this is a reply). Defaults to null.</param>
    /// <param name="FormFile">An optional file attachment (e.g., image or document).</param>
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
