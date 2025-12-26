using FluentValidation.Results;

namespace Comment.Infrastructure.CommonDTOs
{
    /// <summary>
    /// A response model containing a list of validation errors.
    /// </summary>
    /// <param name="msg">A collection of validation failures identified during the validation process.</param>
    public record ValidatorErrorResponse(List<ValidationFailure> msg)
    {
    }
}
