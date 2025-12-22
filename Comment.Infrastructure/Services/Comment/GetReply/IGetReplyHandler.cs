using Comment.Infrastructure.Services.Comment.GetReply.Request;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.GetReply
{
    public interface IGetReplyHandler
    {
        Task<IActionResult> Handle(GetReplyRequest request, CancellationToken cancellationToken);
    }
}