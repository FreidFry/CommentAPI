using Comment.Infrastructure.Services.Comment;
using Comment.Infrastructure.Services.Comment.CreateComment;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("comments")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ICreateCommentHandler _createCommentHandler;


        public CommentController(ICommentService commentService, ICreateCommentHandler createCommentHandler)
        {
            _commentService = commentService;
            _createCommentHandler = createCommentHandler;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get comment by ID", Description = "Retrieves a comment by its unique identifier.")]
        public async Task<IActionResult> GetById([FromQuery]CommentFindDTO dto, CancellationToken cancellationToken)
        {
            return await _commentService.GetByIdAsync(dto, cancellationToken);
        }

        [HttpPost]
        [Authorize]
        [SwaggerOperation(Summary = "Create a new comment", Description = "Creates a new comment with the provided details.")]
        public async Task<IActionResult> Create([FromForm] CommentCreateRequest dto, CancellationToken cancellationToken)
        {
            return await _createCommentHandler.CreateCommentHandleAsync(dto, HttpContext, cancellationToken);
        }

        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Update an existing comment", Description = "Updates an existing comment with the provided details.")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CommentUpdateDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.CommentId)
                return BadRequest("Comment ID mismatch");

            return await _commentService.UpdateAsync(dto, HttpContext, cancellationToken);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete a comment", Description = "Deletes a comment by its unique identifier.")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var dto = new CommentFindDTO(id);
            return await _commentService.DeleteAsync(dto, HttpContext, cancellationToken);
        }
    }
}

