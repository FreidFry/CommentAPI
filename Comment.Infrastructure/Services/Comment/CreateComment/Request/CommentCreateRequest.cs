using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Services.Comment.CreateComment.Request
{
    public record CommentCreateRequest(string Content, Guid ThreadId, Guid? ParentCommentId = null, IFormFile? FormFile = null);
}
