using Comment.Infrastructure.Services.Thread;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("threads")]
    public class ThreadController : ControllerBase
    {
        private readonly IThreadService _threadService;

        public ThreadController(IThreadService threadService)
        {
            _threadService = threadService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetThreads([FromQuery] DateTime? after, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
        {
            var dto = new ThreadsThreeDTO(after, limit);
            return await _threadService.GetThreadsThreeAsync(dto, cancellationToken);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var dto = new ThreadFindDTO(id);
            return await _threadService.GetByIdAsync(dto, cancellationToken);
        }

        [HttpGet("{id}/with-comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByIdWithComments([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var dto = new ThreadFindDTO(id);
            return await _threadService.GetByIdWithCommentsAsync(dto, cancellationToken);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ThreadCreateDTO dto, CancellationToken cancellationToken)
        {
            return await _threadService.CreateAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ThreadUpdateDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.ThreadId)
                return BadRequest("Thread ID mismatch");

            return await _threadService.UpdateAsync(dto, HttpContext, cancellationToken);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var dto = new ThreadFindDTO(id);
            return await _threadService.DeleteAsync(dto, HttpContext, cancellationToken);
        }
    }
}

