using Comment.Infrastructure.Services.User.DTOs.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.User.Validators
{
    public class UserUpdateAvatarValidator : AbstractValidator<UserUpdateAvatarDTO>
    {
        public UserUpdateAvatarValidator()
        {
            RuleFor(x => x.AvatarId)
                .LessThanOrEqualTo((byte)10).WithMessage("AvatarId must be less than or equal to 10.");
        }
    }
}
