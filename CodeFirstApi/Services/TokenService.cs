using CodeFirstApi.Models.Constants;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CodeFirstApi.Services
{
    public interface ITokenService
    {
        string GenerateJwtTokenString(string username);
        string GenerateRefreshTokenString(string username);
        ClaimsPrincipal GetClaimsPrincipalFromJwtToken(string token, out SecurityToken securityToken);
        string GetUsernameFromRefreshToken(string token);
        DateTime SetRefreshTokenExpiry();
    }

    public class TokenService(IConfiguration config, ILogger<TokenService> logger) : ITokenService
    {
        private readonly IConfiguration _config = config;
        private readonly ILogger<TokenService> _logger = logger;

        public string GenerateJwtTokenString(string username)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var expirationTime = DateTime.UtcNow.AddMinutes(1);

                IEnumerable<Claim> claims =
                [
                    new(ClaimTypes.Email,username),
                    new(ClaimTypes.Name, username),
                    new(ClaimTypes.Expiration,expirationTime.ToString("yyyyMMdd_hhmmss")),
                    new(ClaimTypes.Role, Roles.Admin),
                    new(ClaimTypes.Actor, _config.GetSection("Jwt:Actor").Value!)
                ];
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value!));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
                var securityToken = new JwtSecurityToken
                (
                    audience: _config.GetSection("Jwt:Audience").Value,
                    issuer: _config.GetSection("Jwt:Key").Value,
                    claims: claims,
                    notBefore: currentTime,
                    expires: expirationTime,
                    signingCredentials: signingCredentials

                );
                string token = new JwtSecurityTokenHandler().WriteToken(securityToken);
                return token;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Message: {e.StackTrace}");
                throw;
            }
        }

        public ClaimsPrincipal GetClaimsPrincipalFromJwtToken(string token, out SecurityToken securityToken)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateActor = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config.GetSection("Jwt:Issuer").Value,
                    ValidAudience = _config.GetSection("Jwt:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value!))
                };

                var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out securityToken);
                _logger.LogError(JsonConvert.SerializeObject(securityToken));

                return principal;
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }

        public string GenerateRefreshTokenString(string username)
        {
            var randomNumber = new byte[64];
            using (var number = RandomNumberGenerator.Create())
            {
                number.GetBytes(randomNumber);
            }
            var bytes = randomNumber.Concat(Encoding.UTF8.GetBytes(username)).ToArray();
            return Convert.ToBase64String(bytes);
        }

        public string GetUsernameFromRefreshToken(string token)
        {
            var totalBytes = Convert.FromBase64String(token);
            var usernameBytes = totalBytes.Skip(64).ToArray();
            return Encoding.UTF8.GetString(usernameBytes);
        }

        public DateTime SetRefreshTokenExpiry()
        {
            return DateTime.UtcNow.AddSeconds(20);
        }
    }
}
