using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Services.Comment.CreateComment.Request
{
    public record CommentCreateRequestDTO
    {
        public Guid? CallerId { get; set; }
        public string Content { get; set; }
        public Guid ThreadId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public IFormFile? FormFile { get; set; }

        public CommentCreateRequestDTO(Guid? CallerId, CommentCreateRequest request)
        {
            this.CallerId = CallerId;
            Content = request.Content;
            ThreadId = request.ThreadId;
            ParentCommentId = request.ParentCommentId;
            FormFile = request.FormFile;
        }

        public CommentCreateRequestDTO() { }
    };
}
