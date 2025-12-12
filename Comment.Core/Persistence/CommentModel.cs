using System.Text.Json.Serialization;

namespace Comment.Core.Persistence
{
    public class CommentModel(string content, Guid threadId, UserModel user, Guid? parentCommentId = null)
    {
        public Guid Id { get; private set; }
        public string Content { get; private set; } = content;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public Guid ThreadId { get; } = threadId;

        public Guid? ParentCommentId { get; } = parentCommentId;
        [JsonIgnore] public UserModel User { get; } = user;
        public Guid UserId { get; } = user.Id;

        public void UpdateContent(string newContent)
        {
            Content = newContent;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
