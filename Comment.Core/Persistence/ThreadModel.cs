using System.Text.Json.Serialization;

namespace Comment.Core.Persistence
{
    public class ThreadModel
    {
        [JsonInclude] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonInclude] public Guid OwnerId { get; private set; }
        [JsonInclude] public string Title { get; private set; }
        [JsonInclude] public string Context { get; private set; }
        [JsonInclude] public bool IsDeleted { get; private set; } = false;
        [JsonInclude] public bool IsBanned { get; private set; } = false;

        [JsonInclude] public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
        [JsonIgnore] public UserModel OwnerUser { get; }
        [JsonIgnore] public ICollection<CommentModel> Comments { get; } = [];

        public ThreadModel(string title, string context, UserModel user)
        {
            OwnerId = user.Id;
            Title = title;
            Context = context;
            OwnerUser = user;
        }

        [JsonConstructor]
        private ThreadModel()
        {
            
        }
    }
}
