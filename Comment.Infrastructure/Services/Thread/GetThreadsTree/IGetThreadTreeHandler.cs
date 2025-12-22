using Comment.Infrastructure.Services.Thread.GetThreadsTree.Request;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.GetThreadsTree
{
    public interface IGetThreadTreeHandler
    {
        Task<IActionResult> Handle(ThreadsThreeRequest dto, CancellationToken cancellationToken);
    }
}