using CodeFirstApi.Models.Constants;
using CodeFirstApi.Models.Sso;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CodeFirstApi.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }

    public class TokenService(IConfiguration config, ILogger<TokenService> logger) : ITokenService
    {
        private readonly IConfiguration _config = config;
        private readonly ILogger<TokenService> _logger = logger;

        public string GenerateToken(User user)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var expirationTime = DateTime.UtcNow.AddMinutes(1);

                IEnumerable<Claim> claims =
                [
                    new(ClaimTypes.Email,user.Username),
                    new(ClaimTypes.Expiration,expirationTime.ToString("yyyyMMdd_hhmmss")),
                    new(ClaimTypes.Role, Roles.Admin)
                ];
                var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value!));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
                var securityToken = new JwtSecurityToken
                (
                    audience:_config.GetSection("Jwt:Audience").Value,
                    issuer: _config.GetSection("Jwt:Key").Value,
                    claims: claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.AddMinutes(1),
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
    }
}
