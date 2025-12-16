using System.Text.Json.Serialization;

namespace Comment.Core.Persistence
{
    public class ThreadModel
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid OwnerId { get; private set; }
        public string Title { get; private set; }
        public string Context { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public bool IsBanned { get; private set; } = false;

        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
        [JsonIgnore] public UserModel OwnerUser { get; }
        public ICollection<CommentModel> Comments { get; } = [];

        public ThreadModel(string title, string context, UserModel user)
        {
            OwnerId = user.Id;
            Title = title;
            Context = context;
            OwnerUser = user;
        }

        private ThreadModel()
        {
            
        }
    }
}
