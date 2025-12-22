using Comment.Infrastructure.Services.Thread.DTOs;

namespace Comment.Infrastructure.Services.Thread.GetThreadsTree.Response
{
    public record ThreadsTreeResponse(ICollection<ThreadsResponseViewModel> items, DateTime? nextCursor, bool HasMore)
    { }
}
