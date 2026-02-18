using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkplaceTasks.API.Models;
using WorkplaceTasks.API.Services;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace WorkplaceTasks.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _config;

    public AuthController(IUserService userService, IConfiguration config)
    {
        _userService = userService;
        _config = config;
    }

    // POST: api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        var newUser = await _userService.RegisterAsync(user, user.PasswordHash);
        return Ok(new { message = "Registado com sucesso como member", user = newUser.Username });
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User loginData)
    {
        var allUsers = await _userService.GetAllUsersAsync();

        var user = allUsers.FirstOrDefault(u =>
            u.Username == loginData.Username &&
            u.PasswordHash == loginData.PasswordHash);

        if (user == null) return Unauthorized("Credenciais inválidas.");

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            token,
            username = user.Username,
            role = user.Role
        });
    }

    // GET: api/Auth/users
    [HttpGet("users")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return Ok(await _userService.GetAllUsersAsync());
    }

    // PUT: api/Auth/update-role
    [HttpPut("update-role")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateRole(Guid userId, string newRole)
    {
        var result = await _userService.UpdateUserRoleAsync(userId, newRole);
        if (!result) return NotFound("Utilizador não encontrado.");

        return Ok(new { message = "Role atualizada com sucesso." });
    }

    private string GenerateJwtToken(User user)
    {
        var secret = _config["JwtSettings:Secret"]!.Trim();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}