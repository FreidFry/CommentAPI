namespace Comment.Infrastructure.Services.Comment.DTOs.Response
{
    public class CommentTreeDTO
    {
        public ICollection<CommentViewModel> Comments { get; set; } = [];
    }
}

