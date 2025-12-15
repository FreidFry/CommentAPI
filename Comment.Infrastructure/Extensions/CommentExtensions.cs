using Comment.Infrastructure.Services.Comment.DTOs.Response;

namespace Comment.Infrastructure.Extensions
{
    public static class CommentExtensions
    {
        public static List<CommentTreeDTO> BuildCommentTree(List<CommentResponseDTO> comments)
        {
            var commentDict = comments.ToDictionary(c => c.Id, c => new CommentTreeDTO
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ThreadId = c.ThreadId,
                ParentCommentId = c.ParentCommentId,
                UserId = c.UserId,
                UserName = c.UserName,
                AvatarTumbnailUrl = c.AvatarTumbnailUrl,
                Replies = []
            });

            var rootComments = new List<CommentTreeDTO>();

            foreach (var comment in commentDict.Values)
            {
                if (comment.ParentCommentId.HasValue && commentDict.ContainsKey(comment.ParentCommentId.Value))
                    commentDict[comment.ParentCommentId.Value].Replies.Add(comment);
                else
                    rootComments.Add(comment);
            }

            return rootComments;
        }
    }
}
