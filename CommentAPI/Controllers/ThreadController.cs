using Comment.Infrastructure.Services.Comment;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Thread;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("threads")]
    public class ThreadController : ControllerBase
    {
        private readonly IThreadService _threadService;
        private readonly ICommentService _commentService;

        public ThreadController(IThreadService threadService, ICommentService commentService)
        {
            _threadService = threadService;
            _commentService = commentService;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Get Threads",
            Description = "Retrieve a list of threads with optional pagination."
        )]
        public async Task<IActionResult> GetThreads([FromQuery] DateTime? after, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
        {
            var dto = new ThreadsThreeDTO(after, limit);
            return await _threadService.GetThreadsThreeAsync(dto, cancellationToken);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Get Thread by ID",
            Description = "Retrieve a specific thread by its unique identifier."
        )]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var dto = new ThreadFindDTO(id);
            return await _threadService.GetByIdAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPost]
        [Authorize]
        [SwaggerOperation(
            Summary = "Create Thread",
            Description = "Create a new thread with the provided details."
        )]
        public async Task<IActionResult> Create([FromBody] ThreadCreateDTO dto, CancellationToken cancellationToken)
        {
            return await _threadService.CreateAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Update Thread",
            Description = "Update an existing thread with new information."
        )]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ThreadUpdateDTO dto, CancellationToken cancellationToken)
        {
            return await _threadService.UpdateAsync(dto, HttpContext, cancellationToken);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Delete Thread",
            Description = "Delete a thread by its unique identifier."
        )]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            return await _threadService.DeleteAsync(id, HttpContext, cancellationToken);
        }

        [HttpGet("{threadId}/Comments")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Get Comments by Thread",
            Description = "Retrieve comments associated with a specific thread."
        )]
        public async Task<IActionResult> GetByThread(
            [FromRoute] Guid threadId,
            [FromQuery] DateTime? after,
            [FromQuery] int limit = 25,
            CancellationToken cancellationToken = default)
        {
            var dto = new CommentsByThreadDTO(threadId, after, limit);
            return await _commentService.GetByThreadAsync(dto, cancellationToken);
        }

        [HttpPut]
        [Authorize]
        [SwaggerOperation(
            Summary = "Restore Thread",
            Description = "Restore a previously deleted thread by its unique identifier."
        )]
        public async Task<IActionResult> RestoreThreadAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            return await _threadService.RestoreAsync(id, HttpContext, cancellationToken);
        }
    }
}

