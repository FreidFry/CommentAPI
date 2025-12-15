using System.Text.Json.Serialization;

namespace Comment.Core.Persistence
{
    public class CommentModel
    {

        public Guid Id { get; } = Guid.NewGuid();
        public string Content { get; private set; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public bool IsBaned { get; private set; } = false;
        public Guid ThreadId { get; }

        public Guid? ParentCommentId { get; }
        [JsonIgnore] public UserModel User { get; }
        public Guid UserId { get; }

        public CommentModel(string content, Guid threadId, UserModel user, Guid? parentCommentId = null)
        {
            Content = content;
            ThreadId = threadId;
            ParentCommentId = parentCommentId;
            User = user;
            UserId = user.Id;
        }

        public CommentModel()
        {
            
        }

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
