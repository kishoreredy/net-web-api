using Microsoft.AspNetCore.Identity;

namespace CodeFirstApi.Services
{
    public interface IRoleService
    {
        Task<bool> AddRole(string role);
    }

    public class RoleService(RoleManager<IdentityRole> roleManager, ILogger<RoleService> logger) : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ILogger<RoleService> _logger = logger;

        public async Task<bool> AddRole(string role)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                _logger.LogError("Role already exists");
                return false;
            }

            var identityRole = new IdentityRole(role);
            return (await _roleManager.CreateAsync(identityRole)).Succeeded;
        }
    }
}
