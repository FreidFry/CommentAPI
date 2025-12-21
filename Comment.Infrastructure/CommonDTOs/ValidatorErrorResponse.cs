using FluentValidation.Results;

namespace Comment.Infrastructure.CommonDTOs
{
    public record ValidatorErrorResponse(List<ValidationFailure> msg)
    {
    }
}
