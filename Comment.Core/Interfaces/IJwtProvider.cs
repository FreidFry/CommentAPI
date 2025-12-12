using Comment.Core.Persistence;

namespace Comment.Core.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateToken(UserModel user);

    }
}
