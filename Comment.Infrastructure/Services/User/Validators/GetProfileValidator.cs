using Comment.Infrastructure.Services.User.GetProfile.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.User.Validators
{
    public class GetProfileValidator : AbstractValidator<ProfileGetRequestDTO>
    {
        public GetProfileValidator()
        {
            RuleFor(p => p.UserId).NotEmpty();
        }
    }
}
