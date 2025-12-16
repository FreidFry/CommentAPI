using System.Net;
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
        public ThreadModel? Thread { get; }
        public string? ImageUrl { get; private set; }
        public string? ImageTumbnailUrl { get; private set; }


        public Guid? ParentCommentId { get; private set; }
        public CommentModel? ParentComment { get; private set; }
        public int ParentDepth { get; set; } = 0;
        [JsonIgnore] public UserModel User { get; }
        public Guid UserId { get; }
        public ICollection<CommentModel> Replyes { get; } = [];

        public CommentModel(string content, UserModel user, ThreadModel thread, CommentModel? parent = null)
        {
            Content = content;
            Thread = thread;
            ThreadId = thread.Id;
            User = user;
            UserId = user.Id;
            if (parent != null)
            {
                ParentComment = parent;
                ParentCommentId = parent.Id;
                ParentDepth = ParentComment.ParentDepth + 1;
            }
        }

        private CommentModel()
        {
            
        }

        public void AddParent(CommentModel parent)
        {
            ParentComment = parent;
            ParentCommentId = parent.Id;
            ParentDepth = ParentComment.ParentDepth + 1;
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

        public void SetImageUrls(string imageUrl, string imageTumbnailUrl)
        {
            ImageUrl = imageUrl;
            ImageTumbnailUrl = imageTumbnailUrl;
        }
    }
}
