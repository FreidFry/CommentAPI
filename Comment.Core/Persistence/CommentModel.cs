using System.Net;
using System.Text.Json.Serialization;

namespace Comment.Core.Persistence
{
    public class CommentModel
    {

        [JsonInclude] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonInclude] public string Content { get; private set; }
        [JsonInclude] public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        [JsonInclude] public DateTime? UpdatedAt { get; private set; }
        [JsonInclude] public bool IsDeleted { get; private set; } = false;
        [JsonInclude] public bool IsBaned { get; private set; } = false;
        [JsonInclude] public Guid ThreadId { get; private set; }
        [JsonIgnore] public ThreadModel? Thread { get; }
        [JsonInclude] public string? ImageUrl { get; private set; }
        [JsonInclude] public string? ImageTumbnailUrl { get; private set; }
        [JsonInclude] public string? FileUrl { get; private set; }


        [JsonInclude] public Guid? ParentCommentId { get; private set; }
        [JsonIgnore] public CommentModel? ParentComment { get; private set; }
        public int ParentDepth { get; set; } = 0;
        [JsonIgnore] public UserModel User { get; }
        [JsonInclude] public Guid UserId { get; private set; }
        [JsonIgnore] public ICollection<CommentModel> Replyes { get; } = [];

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
        [JsonConstructor]
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

        public void SetFileUrl(string fileUrl)
        {
            FileUrl = fileUrl;
        }
    }
}
