using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Thread.DeleteThread
{
    public class ThreadDeleteValidator : AbstractValidator<DeleteThreadRequestDTO>
    {
        public ThreadDeleteValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("Thread not given.");
            RuleFor(x => x.CallerId)
                .NotEmpty()
                .WithMessage("Please login.");
        }
    }
}
