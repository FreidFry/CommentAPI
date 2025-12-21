using Comment.Infrastructure.Services.Auth.DTOs;
using RestSharp;
using System.Text.Json;

namespace Comment.Infrastructure.Extensions
{
    public static class RestResponseExtensions
    {
        /// <summary>
        /// Attempts to parse the content of a RestResponse into a TokensDTO object.
        /// </summary>
        /// <param name="response">The RestResponse object to parse.</param>
        /// <returns>A tuple indicating success and the parsed TokensDTO object.</returns>
        //public static (bool isSucces, JwtTokenDTO DTO) TryParseTokens(this RestResponse response)
        //{
        //    if (string.IsNullOrEmpty(response.Content))
        //        return (false, new JwtTokenDTO());

        //    using var json = JsonDocument.Parse(response.Content);

        //    if (!json.RootElement.TryGetProperty("access_token", out var tokenEl) ||
        //    !json.RootElement.TryGetProperty("refresh_token", out var refreshEl) ||
        //    !json.RootElement.TryGetProperty("expires_in", out var expiresEl))
        //        return (false, new());
        //    var accessToken = tokenEl.GetString() ?? string.Empty;
        //    var refreshToken = refreshEl.GetString() ?? string.Empty;
        //    var expiresIn = expiresEl.GetInt32();

        //    return (true, new JwtTokenDTO(accessToken, refreshToken, expiresIn));
        //}
    }
}
