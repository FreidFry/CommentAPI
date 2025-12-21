using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Comment.Validators
{
    public class CommentDeleteValidator : AbstractValidator<DeleteCommentRequestDTO>
    {
        public CommentDeleteValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("CommentId required.");
            RuleFor(x => x.CallerId)
                .NotEmpty()
                .WithMessage("Please login.");
        }
    }
}
