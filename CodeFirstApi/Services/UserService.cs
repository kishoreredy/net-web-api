using CodeFirstApi.Models.Sso;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace CodeFirstApi.Services
{
    public interface IUserService
    {
        Task<LoginResponse> Login(User user);
        Task<bool> MapRoleToUser(User user, string role);
        Task<bool> Register(User user);
        Task<LoginResponse> RefreshToken(TokenStrings tokens);
    }

    public class UserService(UserManager<ExtendedIdentityUser> userManager, ITokenService tokenService, ILogger<UserService> logger) : IUserService
    {
        private readonly UserManager<ExtendedIdentityUser> _userManager = userManager;
        private readonly ITokenService _tokenService = tokenService;
        private readonly ILogger<UserService> _logger = logger;

        public async Task<bool> Register(User user)
        {
            if (user == null)
            {
                _logger.LogError("Please enter proper data!");
                return false;
            }

            var identityUser = new ExtendedIdentityUser
            {
                UserName = user.Username,
                Email = user.Username
            };

            var result = await _userManager.CreateAsync(identityUser, user.Password);
            if (result.Succeeded)
            {
                return true;
            }
            _logger.LogError("User registration failed");
            return false;
        }

        public async Task<LoginResponse> Login(User user)
        {
            var response = new LoginResponse();
            if (user == null)
            {
                _logger.LogError("Please enter proper data!");
                return response;
            }

            var identityUser = await _userManager.FindByEmailAsync(user.Username);
            if (identityUser == null)
            {
                _logger.LogError("Unable to find User!");
                return response;
            }

            if (await _userManager.CheckPasswordAsync(identityUser, user.Password))
            {
                response.IsLoggedIn = true;
                response.Tokens = new TokenStrings
                {
                    JwtTokenString = _tokenService.GenerateJwtTokenString(user.Username),
                    RefreshTokenString = _tokenService.GenerateRefreshTokenString(user.Username)
                };

                identityUser.RefreshToken = response.Tokens.RefreshTokenString;
                identityUser.RefreshTokenExpiry = _tokenService.SetRefreshTokenExpiry();

                var updationResult = await _userManager.UpdateAsync(identityUser);
                _logger.LogCritical(updationResult.Succeeded ? "Refresh Token Updated" : "Token updatation failed");

                return response;
            }

            _logger.LogError("Login failed! Password is incorrect!");
            return response;
        }

        public async Task<LoginResponse> RefreshToken(TokenStrings tokens)
        {
            var response = new LoginResponse();
            var refreshTokenUsername = _tokenService.GetUsernameFromRefreshToken(tokens.RefreshTokenString);
            var principal = _tokenService.GetClaimsPrincipalFromJwtToken(tokens.JwtTokenString, out SecurityToken securityToken);
            var principalIdentityName = principal?.Identity?.Name;
            if (principalIdentityName == null || !principalIdentityName.Equals(refreshTokenUsername))
                return response;

            var jwtSecurityToken = (JwtSecurityToken)securityToken;
            //if(jwtSecurityToken == null || jwtSecurityToken.Claims.Contains(c=>c.)

            var identityUser = await _userManager.FindByNameAsync(principalIdentityName);
            if (identityUser == null || !refreshTokenUsername.Equals(identityUser.UserName) ||
                identityUser.RefreshToken != tokens.RefreshTokenString || identityUser.RefreshTokenExpiry < DateTime.UtcNow)
                return response;

            response.IsLoggedIn = true;
            response.Tokens = new TokenStrings
            {
                JwtTokenString = _tokenService.GenerateJwtTokenString(identityUser.UserName),
                RefreshTokenString = _tokenService.GenerateRefreshTokenString(identityUser.UserName)
            };

            identityUser.RefreshToken = response.Tokens.RefreshTokenString;
            identityUser.RefreshTokenExpiry = _tokenService.SetRefreshTokenExpiry();
            var updationResult = await _userManager.UpdateAsync(identityUser);
            _logger.LogCritical(updationResult.Succeeded ? "Refresh Token Updated" : "Token updatation failed");

            return response;
        }

        public async Task<bool> MapRoleToUser(User user, string role)
        {
            if (user == null)
            {
                _logger.LogError("Please enter proper data!");
                return false;
            }

            var identityUser = await _userManager.FindByEmailAsync(user.Username);
            if (identityUser == null)
            {
                _logger.LogError("Unable to find User!");
                return false;
            }

            var result = await _userManager.AddToRoleAsync(identityUser, role);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role mapped to User!");
                return true;
            }

            _logger.LogError("Unable to map role to the User!");
            return false;
        }
    }
}
