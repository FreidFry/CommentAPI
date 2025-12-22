using Comment.Infrastructure.Services.Thread.UpdateThread.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Thread.UpdateThread
{
    public class ThreadUpdateValidator : AbstractValidator<UpdateThreadRequestDTO>
    {
        public ThreadUpdateValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId is required.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(200)
                .WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Context)
                .NotEmpty()
                .WithMessage("Context is required.")
                .MaximumLength(5000)
                .WithMessage("Context must not exceed 5000 characters.");

            RuleFor(x => x.CallerId)
                .NotEmpty()
                .WithMessage("Please login");
        }
    }
}

