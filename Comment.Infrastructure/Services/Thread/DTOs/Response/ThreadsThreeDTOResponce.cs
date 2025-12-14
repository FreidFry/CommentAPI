namespace Comment.Infrastructure.Services.Thread.DTOs.Response
{
    public record ThreadsThreeDTOResponce(Guid Id, string Title, string Content, DateTime CreatedAt, int CommentCount);
}
