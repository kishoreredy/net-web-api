using CodeFirstApi.Models.Constants;
using CodeFirstApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeFirstApi.Controllers.Sso
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class RolesController(IRoleService roleService) : ControllerBase
    {
        private readonly IRoleService _roleService = roleService;

        [HttpPost]
        public async Task<IActionResult> AddRole(string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest($"State: {ModelState} Validation State: {ModelState.ValidationState}");
            }

            return await _roleService.AddRole(role)
                ? Ok($"{role} role is added.")
                : BadRequest("Something went wrong!");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRole(string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest($"State: {ModelState} Validation State: {ModelState.ValidationState}");
            }

            return await _roleService.DeleteRole(role)
                ? Ok($"{role} role is deleted.")
                : BadRequest("Something went wrong!");
        }
    }
}
