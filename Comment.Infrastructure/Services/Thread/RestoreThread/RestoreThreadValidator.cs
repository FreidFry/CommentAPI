using FluentValidation;

namespace Comment.Infrastructure.Services.Thread.RestoreThread
{
    public class RestoreThreadValidator : AbstractValidator<RestoreThreadRequestDTO>
    {
        public RestoreThreadValidator()
        {
            RuleFor(x => x.ThreadId)
                .NotEmpty()
                .WithMessage("ThreadId required.");
            RuleFor(x => x.CallerId)
                .NotEmpty()
                .WithMessage("Please login.");
        }
    }
}
