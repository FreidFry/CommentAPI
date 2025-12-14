using Comment.Infrastructure.Services.Thread.DTOs.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Thread.Validators
{
    public class ThreadFindValidator : AbstractValidator<ThreadFindDTO>
    {
        public ThreadFindValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId is required.");
        }
    }
}

