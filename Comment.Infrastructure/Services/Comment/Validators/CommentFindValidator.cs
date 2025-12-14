using Comment.Infrastructure.Services.Comment.DTOs.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Comment.Validators
{
    public class CommentFindValidator : AbstractValidator<CommentFindDTO>
    {
        public CommentFindValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("CommentId is required.");
        }
    }
}

