using Comment.Infrastructure.Services.Thread.CreateThread.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.Thread.CreateThread
{
    public class ThreadCreateValidator : AbstractValidator<ThreadCreateRequestDTO>
    {
        public ThreadCreateValidator()
        {
            RuleFor(x => x.callerId)
                .NotEmpty()
                .WithMessage("Please login.");
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
        }
    }
}

