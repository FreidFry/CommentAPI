using Comment.Infrastructure.Services.Comment.GetCommentsByThread;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request;
using Comment.Infrastructure.Services.Thread.CreateThread;
using Comment.Infrastructure.Services.Thread.CreateThread.Request;
using Comment.Infrastructure.Services.Thread.DeleteThread;
using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Request;
using Comment.Infrastructure.Services.Thread.GetThreadsTree;
using Comment.Infrastructure.Services.Thread.GetThreadsTree.Request;
using Comment.Infrastructure.Services.Thread.RestoreThread;
using Comment.Infrastructure.Services.Thread.RestoreThread.Request;
using Comment.Infrastructure.Services.Thread.UpdateThread;
using Comment.Infrastructure.Services.Thread.UpdateThread.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("threads")]
    public class ThreadController : ControllerBase
    {
        private readonly IGetCommentsByThreadHandler _getCommentsByThreadHandler;
        private readonly ICreateThreadHandler _createThreadHandler;
        private readonly IGetDetailedThreadHandler _getDetailedThreadHandler;
        private readonly IGetThreadTreeHandler _getThreadTreeHandler;
        private readonly IUpdateThreadHandler _updateThreadHandler;
        private readonly IDeleteThreadHandler _deleteThreadHandler;
        private readonly IRestoreThreadHandler _restoreThreadHandler;

        public ThreadController(
            IGetCommentsByThreadHandler getCommentsByThreadHandler,
            ICreateThreadHandler createThreadHandler,
            IGetDetailedThreadHandler getDetailedThreadHandler,
            IGetThreadTreeHandler getThreadTreeHandler,
            IUpdateThreadHandler updateThreadHandler,
            IDeleteThreadHandler deleteThreadHandler,
            IRestoreThreadHandler restoreThreadHandler)
        {
            _getCommentsByThreadHandler = getCommentsByThreadHandler;
            _createThreadHandler = createThreadHandler;
            _getDetailedThreadHandler = getDetailedThreadHandler;
            _getThreadTreeHandler = getThreadTreeHandler;
            _updateThreadHandler = updateThreadHandler;
            _deleteThreadHandler = deleteThreadHandler;
            _restoreThreadHandler = restoreThreadHandler;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Get Threads",
            Description = "Retrieve a list of threads with optional pagination."
        )]
        public async Task<IActionResult> GetThreads([FromQuery] ThreadsThreeRequest dto, CancellationToken cancellationToken = default)
        {
            return await _getThreadTreeHandler.Handle(dto, cancellationToken);
        }

        [HttpGet("{ThreadId}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Get Thread by ID",
            Description = "Retrieve a specific thread by its unique identifier."
        )]
        public async Task<IActionResult> GetById([FromRoute] ThreadDetaliRequest dto, CancellationToken cancellationToken)
        {
            return await _getDetailedThreadHandler.Handle(dto, HttpContext, cancellationToken);
        }

        [HttpPost]
        [Authorize]
        [SwaggerOperation(
            Summary = "Create Thread",
            Description = "Create a new thread with the provided details."
        )]
        public async Task<IActionResult> Create([FromBody] ThreadCreateRequest dto, CancellationToken cancellationToken)
        {
            return await _createThreadHandler.Handle(dto, HttpContext, cancellationToken);
        }

        [HttpPut("{threadId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Update Thread",
            Description = "Update an existing thread with new information."
        )]
        public async Task<IActionResult> Update([FromRoute] Guid threadId, [FromBody] UpdateThreadBody request, CancellationToken cancellationToken)
        {
            var dto = new UpdateThreadRequest(threadId, request.Title, request.Context);
            return await _updateThreadHandler.Handle(dto, HttpContext, cancellationToken);
        }

        [HttpDelete("{ThreadId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Delete Thread",
            Description = "Delete a thread by its unique identifier."
        )]
        public async Task<IActionResult> Delete([FromRoute] DeleteThreadRequest request, CancellationToken cancellationToken)
        {
            return await _deleteThreadHandler.Handle(request, HttpContext, cancellationToken);
        }

        [HttpGet("{threadId}/Comments")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Get Comments by Thread",
            Description = "Retrieve comments associated with a specific thread."
        )]
        public async Task<IActionResult> GetByThread([FromRoute] Guid threadId, [FromQuery] CommentsByThreadRequest dto, CancellationToken cancellationToken)
        {
            return await _getCommentsByThreadHandler.Handle(threadId, dto, cancellationToken);
        }

        [HttpPut("{ThreadId}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Restore Thread",
            Description = "Restore a previously deleted thread by its unique identifier."
        )]
        public async Task<IActionResult> RestoreThreadAsync([FromRoute] RestoreThreadRequest request, CancellationToken cancellationToken)
        {
            return await _restoreThreadHandler.Handle(request, HttpContext, cancellationToken);
        }
    }
}

