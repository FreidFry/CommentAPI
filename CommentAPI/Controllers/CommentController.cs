using Comment.Infrastructure.Services.Comment;
using Comment.Infrastructure.Services.Comment.CreateComment;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.DeleteComment;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.UpdateComment;
using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
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
        private readonly IUpdateCommentHandler _updateCommentHandler;
        private readonly IDeleteCommentHandler _deleteCommentHandler;


        public CommentController(ICommentService commentService, ICreateCommentHandler createCommentHandler, IUpdateCommentHandler updateCommentHandler, IDeleteCommentHandler deleteCommentHandler)
        {
            _commentService = commentService;
            _createCommentHandler = createCommentHandler;
            _updateCommentHandler = updateCommentHandler;
            _deleteCommentHandler = deleteCommentHandler;
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

        [HttpPut]
        [Authorize]
        [SwaggerOperation(Summary = "Update an existing comment", Description = "Updates an existing comment with the provided details.")]
        public async Task<IActionResult> Update([FromBody] CommentUpdateRequest dto, CancellationToken cancellationToken)
        {
            return await _updateCommentHandler.UpdateCommentHandle(dto, HttpContext, cancellationToken);
        }

        [HttpDelete("{CommentId}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete a comment", Description = "Deletes a comment by its unique identifier.")]
        public async Task<IActionResult> Delete([FromRoute] DeleteCommentRequest dto, CancellationToken cancellationToken)
        {
            return await _deleteCommentHandler.DeleteCommentHandle(dto, HttpContext, cancellationToken);
        }
    }
}

