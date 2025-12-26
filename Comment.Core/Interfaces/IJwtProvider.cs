using Comment.Core.Persistence;

namespace Comment.Core.Interfaces
{
    /// <summary>
    /// Defines a contract for an authentication token provider responsible for generating 
    /// JWT based on user identity data.
    /// </summary>
    public interface IJwtProvider
    {
        /// <summary>
        /// Generates an encoded JWT string containing the specified user's identity claims.
        /// </summary>
        /// <param name="user">The user model containing identity information and roles.</param>
        /// <returns>A signed JWT string that can be used for client authentication.</returns>
        string GenerateToken(UserModel user);

    }
}
