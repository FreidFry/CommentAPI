using Comment.Core.Persistence;

namespace Comment.Infrastructure.Services.Auth.Login.Response
{
    public record LoginSuccesResponse(Guid? Id, string? UserName, List<string>? Roles, UserModel UserModel);
}
