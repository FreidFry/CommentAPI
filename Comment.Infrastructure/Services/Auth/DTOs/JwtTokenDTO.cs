namespace Comment.Infrastructure.Services.Auth.DTOs
{
    public class JwtTokenDTO
    {
        /// <summary>
        /// Gets the access token.
        /// </summary>
        public string Access_Token { get; private set; }
        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        public string Refresh_Token { get; private set; }
        /// <summary>
        /// Gets the expiration time in seconds.
        /// </summary>
        public int Expires_In { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TokensDTO"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="expiresAt">The expiration time in seconds.</param>
        public JwtTokenDTO(string accessToken, string refreshToken, int expiresAt)
        {
            Access_Token = accessToken;
            Refresh_Token = refreshToken;
            Expires_In = expiresAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokensDTO"/> class.
        /// </summary>
        public JwtTokenDTO()
        {
            Access_Token = string.Empty;
            Refresh_Token = string.Empty;
            Expires_In = 0;
        }
    }
}
