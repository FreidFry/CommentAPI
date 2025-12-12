using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Auth.DTOs;
using RestSharp;

namespace Comment.Infrastructure.Storages
{
    public class TokenStorage
    {
        string AccessToken = string.Empty;
        string RefreshToken = string.Empty;
        DateTime ExpiresAt;
        private Timer? _tokenCheckTimer;
        private bool _disposed = false;

        private const int TOKEN_EXPIRY_BUFFER_MINUTES = 5;
        private const int TOKEN_CHECK_INTERVAL_MINUTES = 1;
        private const int TOKEN_WARNING_MINUTES = 7;

        public string Access_Token
        {
            get => AccessToken;
            private set => AccessToken = value;
        }

        public string Refresh_Token
        {
            get => RefreshToken;
            private set => RefreshToken = value;
        }

        public DateTime Expires_At
        {
            get => ExpiresAt;
            private set => ExpiresAt = value;
        }

        public bool ParseAndSetTokens(RestResponse response)
        {
            if (string.IsNullOrEmpty(response.Content))
                return false;
            var (isSucces, DTO) = response.TryParseTokens();

            if (isSucces)
            {
                SetToken(DTO);
                return false;
            }

            return false;
        }

        private void SetToken(JwtTokenDTO tokenDTO)
        {
            Access_Token = tokenDTO.Access_Token;
            Refresh_Token = tokenDTO.Refresh_Token;
            Expires_At = DateTime.UtcNow.AddSeconds(tokenDTO.Expires_In - TOKEN_EXPIRY_BUFFER_MINUTES * 60);

            StartTokenCheckTimer();
        }

        private void StartTokenCheckTimer()
        {
            _tokenCheckTimer?.Dispose();

            if (!string.IsNullOrEmpty(Access_Token))
            {
                _tokenCheckTimer = new Timer(CheckTokenExpiration, null, TimeSpan.Zero, TimeSpan.FromMinutes(TOKEN_CHECK_INTERVAL_MINUTES));
            }
        }

        private void CheckTokenExpiration(object? state)
        {
            if (_disposed) return;

            if (IsAccessTokenExpired())
            {
                Console.WriteLine("[TokenStorage] Token expired, triggering refresh event");
                IsAccessTokenExpiredEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var timeUntilExpiry = Expires_At - DateTime.UtcNow;
                if (timeUntilExpiry.TotalMinutes <= TOKEN_WARNING_MINUTES)
                {
                    Console.WriteLine($"[TokenStorage] Token expires in {timeUntilExpiry.TotalMinutes:F1} minutes");
                }
            }
        }

        public bool IsAccessTokenExpired() => DateTime.UtcNow >= Expires_At;

        public event EventHandler? IsAccessTokenExpiredEvent;

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _tokenCheckTimer?.Dispose();
        }
    }
}
