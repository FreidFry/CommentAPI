using Comment.Infrastructure.Enums;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request
{
    public record CommentsByThreadRequestDTO
    {
        public Guid ThreadId { get; set; }
        public SortByEnum SortByEnum { get; set; }
        public bool IsAscending { get; set; }
        public string? After { get; set; }
        public int Limit { get; set; }
        public Guid? FocusCommentId { get; set; }

        public CommentsByThreadRequestDTO(Guid guid, CommentsByThreadRequest request)
        {
            ThreadId = guid;
            SortByEnum = request.SortByEnum;
            IsAscending = request.IsAscending;
            After = request.After;
            Limit = request.Limit;
            FocusCommentId = request.FocusCommentId;
        }

        public CommentsByThreadRequestDTO() { }
    }
}
