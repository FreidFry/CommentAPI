using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Services.Comment.DTOs.Request
{
    public record CommentCreateDTO(string Content, Guid ThreadId, Guid? ParentCommentId = null, IFormFile? FormFile = null);
}
