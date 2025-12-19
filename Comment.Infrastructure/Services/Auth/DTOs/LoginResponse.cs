using Comment.Core.Persistence;

namespace Comment.Infrastructure.Services.Auth.DTOs
{
    public record LoginResponse(Guid? Id, string? UserName, List<string>? Roles, UserModel UserModel);
}
