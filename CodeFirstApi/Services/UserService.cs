using CodeFirstApi.Models.Sso;
using Microsoft.AspNetCore.Identity;

namespace CodeFirstApi.Services
{
    public interface IUserService
    {
        Task<bool> Login(User user);
        Task<bool> MapRoleToUser(User user, string role);
        Task<bool> Register(User user);
    }

    public class UserService(UserManager<IdentityUser> userManager, ILogger<UserService> logger) : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly ILogger<UserService> _logger = logger;

        public async Task<bool> Register(User user)
        {
            if (user == null)
            {
                _logger.LogError("Please enter proper data!");
                return false;
            }

            var identityUser = new IdentityUser
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

        public async Task<bool> Login(User user)
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

            return await _userManager.CheckPasswordAsync(identityUser, user.Password);
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
