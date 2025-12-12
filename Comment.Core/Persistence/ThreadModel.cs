using System.Text.Json.Serialization;

namespace Comment.Core.Persistence
{
    public class ThreadModel(string title, string context, UserModel user)
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid OwnerId { get; private set; } = user.Id;
        public string Title { get; private set; } = title;
        public string Context { get; private set; } = context;
        public bool IsDeleted { get; private set; } = false;

        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; }
        [JsonIgnore] public UserModel OwnerUser { get; } = user;
        public ICollection<CommentModel> Comments { get; } = [];
    }
}
