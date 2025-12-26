using Comment.Core.Persistence;

namespace Comment.Infrastructure.Services.Auth.Login.Response
{
    /// <summary>
    /// Represents a successful login response containing user identity and authorization details.
    /// This data is typically used to issue authentication cookies or JWT tokens.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <param name="UserName">The user's login or display name.</param>
    /// <param name="Roles">A list of security roles assigned to the user.</param>
    /// <param name="UserModel">The full user profile data model. (To create a JWT, it is not sent to the client.)</param>
    public record LoginSuccesResponse(Guid? Id, string? UserName, List<string>? Roles, UserModel UserModel);
}
