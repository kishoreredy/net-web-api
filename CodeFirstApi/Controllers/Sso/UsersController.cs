using CodeFirstApi.Models.Constants;
using CodeFirstApi.Models.Sso;
using CodeFirstApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeFirstApi.Controllers.Sso
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController(IUserService userService, ITokenService tokenService) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;

        [HttpPost("Register")]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest($"State: {ModelState} Validation State: {ModelState.ValidationState}");
            }

            return await _userService.Register(user)
                ? Ok($"User registration is done.")
                : BadRequest("Something went wrong!");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest($"State: {ModelState} Validation State: {ModelState.ValidationState}");
            }

            bool status = await _userService.Login(user);
            if (status)
            {
                string token = _tokenService.GenerateToken(user);
                return Ok($"token: {token}");
            }

            return BadRequest("Invalid Credentials!");
        }

        [HttpPost("MapUserToRole")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> MapRoleToUser(User user, string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest($"State: {ModelState} Validation State: {ModelState.ValidationState}");
            }

            return await _userService.MapRoleToUser(user, role)
                ? Ok("Role mapped.")
                : BadRequest("Something went wrong!");

        }
    }
}