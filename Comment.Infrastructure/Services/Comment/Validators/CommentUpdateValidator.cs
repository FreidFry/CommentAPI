using Comment.Infrastructure.Services.Comment.DTOs.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Comment.Validators
{
    public class CommentUpdateValidator : AbstractValidator<CommentUpdateDTO>
    {
        public CommentUpdateValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("CommentId is required.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required.")
                .MaximumLength(5000)
                .WithMessage("Content must not exceed 5000 characters.");
        }
    }
}

