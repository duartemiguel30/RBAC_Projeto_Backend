using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WorkplaceTasks.API.Data;
using WorkplaceTasks.API.Models;

namespace WorkplaceTasks.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // TODO: Utilizar hash das passwords
    public (string Token, string Role)? Authenticate(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u =>
            u.Username.ToLower() == username.ToLower() &&
            u.PasswordHash == password);

        if (user == null) return null;

        var secretKey = _configuration["JwtSettings:Secret"]?.Trim()
            ?? throw new InvalidOperationException("JWT Secret missing.");

        var expirationHours = double.Parse(_configuration["JwtSettings:ExpirationInHours"] ?? "8");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenHandler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(expirationHours),
            SigningCredentials = creds
        };

        var tokenString = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenString, user.Role);
    }
}
