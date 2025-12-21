using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Comment.Validators
{
    public class CommentCreateValidator : AbstractValidator<CommentCreateRequestDTO>
    {
        public CommentCreateValidator()
        {
            RuleFor(x => x.CallerId)
                .NotEmpty();
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required.")
                .MaximumLength(5000)
                .WithMessage("Content must not exceed 5000 characters.");

            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId is required.");

            RuleFor(x => x.FormFile)
                .Must(file => file == null || file.Length <= 2 * 1024 * 1024) // 2 MB limit
                .WithMessage("FormFile size must not exceed 2 MB.");
        }
    }
}

