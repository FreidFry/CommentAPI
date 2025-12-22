using Comment.Core.Persistence;

namespace Comment.Infrastructure.Services.Auth.Register.Response
{
    public record RegisterSuccesResult(Guid Id, string UserName, ICollection<string> Roles, UserModel UserModel)
    {
    }
}
