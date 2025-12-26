using Comment.Infrastructure.Services.User.GetProfile.Request;
using FluentValidation;

namespace Comment.Infrastructure.Services.User.GetProfile
{
    public class GetProfileValidator : AbstractValidator<ProfileGetRequestDTO>
    {
        public GetProfileValidator()
        {
            RuleFor(p => p.UserId).NotEmpty();
        }
    }
}
