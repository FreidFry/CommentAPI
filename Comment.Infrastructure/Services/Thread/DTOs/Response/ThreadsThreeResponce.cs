namespace Comment.Infrastructure.Services.Thread.DTOs.Response
{
    public record ThreadsThreeResponce(Guid Id, string Title, string Content, DateTime CreatedAt, int CommentCount);
}
