using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response;

namespace Comment.Infrastructure.Services.Comment.DTOs.Response
{
    public class CommentTreeDTO
    {
        public ICollection<CommentViewModel> Comments { get; set; } = [];
    }
}

