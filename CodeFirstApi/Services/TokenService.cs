using CodeFirstApi.Models.Constants;
using Microsoft.IdentityModel.Tokens;
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
        bool SecurityTokenClaimsValidation(SecurityToken securityToken, string username);
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
                var expirationTime = DateTime.UtcNow.AddSeconds(Convert.ToInt16(_config.GetSection("TokenExpiration:Jwt").Value));

                IEnumerable<Claim> claims =
                [
                    new(ClaimTypes.Email,username),
                    new(ClaimTypes.Name, username),
                    new(ClaimTypes.Expiration,expirationTime.ToString("yyyyMMdd_HHmmss")),
                    new(ClaimTypes.Role, Roles.Admin),
                    new(ClaimTypes.Actor, _config.GetSection("Jwt:Actor").Value!)
                ];
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value!));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
                var securityToken = new JwtSecurityToken
                (
                    audience: _config.GetSection("Jwt:Audience").Value,
                    issuer: _config.GetSection("Jwt:Issuer").Value,
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
                    //ClockSkew = TimeSpan.Zero,
                    ValidIssuer = _config.GetSection("Jwt:Issuer").Value,
                    ValidAudience = _config.GetSection("Jwt:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value!))
                };

                var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out securityToken);

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
            return DateTime.UtcNow.AddSeconds(Convert.ToInt16(_config.GetSection("TokenExpiration:Refresh").Value));
        }

        public bool SecurityTokenClaimsValidation(SecurityToken securityToken, string username)
        {
            try
            {
                var jwtSecurityToken = (JwtSecurityToken)securityToken;
                if (jwtSecurityToken == null) return false;

                //bool result = true;
                foreach (var claim in jwtSecurityToken.Claims)
                {
                    switch (claim.Type)
                    {
                        case ClaimTypes.Name:
                            if (claim.Value.Equals(username))
                            {
                                continue;
                            }
                            //result = false;
                            //goto end_foreach;
                            return false;
                        case ClaimTypes.Role:
                            if (claim.Value.Equals(Roles.Admin))
                            {
                                continue;
                            }
                            //result = false;
                            //goto end_foreach;
                            return false;
                        case ClaimTypes.Email:
                            if (claim.Value.Equals(username))
                            {
                                continue;
                            }
                            //result = false;
                            //goto end_foreach;
                            return false;
                        case ClaimTypes.Expiration:
                            var expireTime = DateTime.ParseExact(claim.Value, "yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                            if (expireTime > DateTime.UtcNow)
                            {
                                continue;
                            }
                            //result = false;
                            //goto end_foreach;
                            return false;
                        case ClaimTypes.Actor:
                            if (claim.Value.Equals(_config.GetSection("Jwt:Actor").Value))
                            {
                                continue;
                            }
                            //result = false;
                            //goto end_foreach;
                            return false;
                        default:
                            continue;
                    }
                //end_foreach: break; //break in switch block is not exiting the complete foreach loop, so goto is used
                }

                //return result;
                return true;
            }
            catch (InvalidCastException ic)
            {
                _logger.LogError(ic.StackTrace);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }
        }
    }
}
