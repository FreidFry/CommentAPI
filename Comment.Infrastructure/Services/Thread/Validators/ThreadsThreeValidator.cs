using Comment.Infrastructure.Services.Thread.DTOs.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Thread.Validators
{
    public class ThreadsThreeValidator : AbstractValidator<ThreadsThreeDTO>
    {
        public ThreadsThreeValidator()
        {
            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .WithMessage("Limit must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Limit must not exceed 100.");
        }
    }
}

