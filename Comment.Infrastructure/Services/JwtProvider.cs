using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Comment.Infrastructure.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly IJwtOptions _options;

        public JwtProvider(IJwtOptions options)
        {
            _options = options;
        }

        public string GenerateToken(UserModel user)
        {
            Claim[] claims = [new("uid", user.Id.ToString())];

            var singningKey = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256
                );

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                signingCredentials: singningKey,
                expires: DateTime.UtcNow.AddDays(_options.ExpiresDays)
                );

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenValue;
        }
    }
}


