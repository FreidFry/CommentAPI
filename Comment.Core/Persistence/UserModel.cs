using System.Text.Json.Serialization;

namespace Comment.Core.Persistence
{
    /// <summary>
    /// Represents a user account, including identity, profile information, roles, and account status.
    /// </summary>
    public class UserModel(string userName, string email, string hashPassword, string? homePage = null)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = userName;
        public string Email { get; set; } = email;
        public string? HomePage { get; set; } = homePage;
        public string HashPassword { get;set; } = hashPassword;
        public ICollection<string> Roles { get; set; } = [];
        public bool IsDeleted { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public string AvatarUrl { get; set; } = string.Empty;
        public string AvatarTumbnailUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; }

        [JsonIgnore] public ICollection<CommentModel> Comments { get; set; } = [];
        [JsonIgnore] public ICollection<ThreadModel> Threads { get; set; } = [];

        public void MarkAsDeleted() => IsDeleted = true;
        public void BanUser() => IsBanned = true;
    }
}
