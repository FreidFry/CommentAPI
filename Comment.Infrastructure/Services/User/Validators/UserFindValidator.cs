using Comment.Infrastructure.Services.User.DTOs.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.User.Validators
{
    public class UserFindValidator : AbstractValidator<UserFindDto>
    {
        public UserFindValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");
        }
    }
}
