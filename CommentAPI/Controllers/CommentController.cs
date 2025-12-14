using Comment.Infrastructure.Services.Comment;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("comments")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("thread/{threadId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByThread(
            [FromRoute] Guid threadId,
            [FromQuery] DateTime? after,
            [FromQuery] int limit = 50,
            CancellationToken cancellationToken = default)
        {
            var dto = new CommentsByThreadDTO(threadId, after, limit);
            return await _commentService.GetByThreadAsync(dto, cancellationToken);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var dto = new CommentFindDTO(id);
            return await _commentService.GetByIdAsync(dto, cancellationToken);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CommentCreateDTO dto, CancellationToken cancellationToken)
        {
            return await _commentService.CreateAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CommentUpdateDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.CommentId)
                return BadRequest("Comment ID mismatch");

            return await _commentService.UpdateAsync(dto, HttpContext, cancellationToken);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var dto = new CommentFindDTO(id);
            return await _commentService.DeleteAsync(dto, HttpContext, cancellationToken);
        }
    }
}

