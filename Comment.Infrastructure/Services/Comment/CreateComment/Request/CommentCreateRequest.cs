using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Services.Comment.CreateComment.Request
{
    public record CommentCreateRequest(string Content, Guid ThreadId, string CaptchaId, string CaptchaValue, Guid? ParentCommentId = null, IFormFile? FormFile = null);
}
