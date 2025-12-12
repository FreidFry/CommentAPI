using Comment.Infrastructure.Services.User.DTOs.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.User.Validators
{
    public class UserCreateValidator : AbstractValidator<UserCreateDTO>
    {
        public UserCreateValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required.");
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("A valid email is required.");
            RuleFor(x => x.HashPassword)
                .NotEmpty()
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long.");
            RuleFor(x => x.HomePage)
                .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                .When(x => !string.IsNullOrWhiteSpace(x.HomePage))
                .WithMessage("A valid address format.");
        }
    }
}
