using CodeFirstApi.Models.Constants;
using CodeFirstApi.Models.Sso;
using CodeFirstApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeFirstApi.Controllers.Sso
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

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

            var response = await _userService.Login(user);
            if (response.IsLoggedIn)
            {
                HttpContext.Session.SetString(user.Username, System.Text.Json.JsonSerializer.Serialize(response));
                return Ok(response);
            }

            return BadRequest("Invalid Credentials!");
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(TokenStrings tokens)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest($"State: {ModelState} Validation State: {ModelState.ValidationState}");
            }

            var response = await _userService.RefreshToken(tokens);
            return response.IsLoggedIn
                ? Ok(response)
                : Unauthorized("Token Invalid! Please login!");
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