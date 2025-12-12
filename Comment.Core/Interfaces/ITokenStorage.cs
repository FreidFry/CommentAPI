using RestSharp;

namespace Comment.Core.Interfaces
{
    public interface ITokenStorage : IDisposable
    {
        string Access_Token { get; }
        string Refresh_Token { get; }
        DateTime Expires_At { get; }

        event EventHandler? IsAccessTokenExpiredEvent;

        bool IsAccessTokenExpired();
        bool ParseAndSetTokens(RestResponse response);
    }
}
