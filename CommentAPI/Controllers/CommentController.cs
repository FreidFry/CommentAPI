using Comment.Infrastructure.Services.Comment.CreateComment;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.DeleteComment;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using Comment.Infrastructure.Services.Comment.GetReply;
using Comment.Infrastructure.Services.Comment.GetReply.Request;
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
        private readonly ICreateCommentHandler _createCommentHandler;
        private readonly IUpdateCommentHandler _updateCommentHandler;
        private readonly IDeleteCommentHandler _deleteCommentHandler;
        private readonly IGetReplyHandler _getReplyHandler;

        public CommentController(ICreateCommentHandler createCommentHandler, IUpdateCommentHandler updateCommentHandler, IDeleteCommentHandler deleteCommentHandler, IGetReplyHandler getReplyHandler)
        {
            _createCommentHandler = createCommentHandler;
            _updateCommentHandler = updateCommentHandler;
            _deleteCommentHandler = deleteCommentHandler;
            _getReplyHandler = getReplyHandler;
        }

        [HttpGet("replyes")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get comment by ID", Description = "Retrieves a comment by its unique identifier.")]
        public async Task<IActionResult> GetById([FromQuery] GetReplyRequest dto, CancellationToken cancellationToken)
        {
            return await _getReplyHandler.Handle(dto, cancellationToken);
        }

        [HttpPost]
        [Authorize]
        [SwaggerOperation(Summary = "Create a new comment", Description = "Creates a new comment with the provided details.")]
        public async Task<IActionResult> Create([FromForm] CommentCreateRequest dto, CancellationToken cancellationToken)
        {
            return await _createCommentHandler.Handle(dto, HttpContext, cancellationToken);
        }

        [HttpPut("{CommentId}")]
        [Authorize]
        [SwaggerOperation(Summary = "Update an existing comment", Description = "Updates an existing comment with the provided details.")]
        public async Task<IActionResult> Update([FromRoute]Guid CommentId, [FromBody] CommentUpdateBody request, CancellationToken cancellationToken)
        {
            var dto = new CommentUpdateRequest(CommentId, request.Content);
            return await _updateCommentHandler.Handle(dto, HttpContext, cancellationToken);
        }

        [HttpDelete("{CommentId}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete a comment", Description = "Deletes a comment by its unique identifier.")]
        public async Task<IActionResult> Delete([FromRoute] DeleteCommentRequest dto, CancellationToken cancellationToken)
        {
            return await _deleteCommentHandler.Handle(dto, HttpContext, cancellationToken);
        }
    }
}

