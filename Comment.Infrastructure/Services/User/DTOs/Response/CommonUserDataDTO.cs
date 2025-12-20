using Comment.Infrastructure.Services.Thread.DTOs.Response;

namespace Comment.Infrastructure.Services.User.DTOs.Response
{
    public class CommonUserDataDTO
    {
        public string UserName { get; set; }
        public string AvatarTumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string HomePage { get; set; }
        public DateTime? LastActive { get; set; }
        public ICollection<DetailedThreadResponse> Threads { get; set; } = [];

}
}
