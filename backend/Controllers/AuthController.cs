using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TrainingService.Controllers;

public record LoginRequest(string Username, string Password);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config) => _config = config;

    // ── Static users (username → password, roles, orgId) ─────────────────────
    private static readonly Dictionary<string, (string Password, string[] Roles, int OrgId)> _users = new()
    {
        ["admin"]   = ("Admin@123",   ["Training", "Developer"], 1),
        ["trainer"] = ("Trainer@123", ["Training"],              1),
        ["trainee"] = ("Trainee@123", [],                        1),
    };

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username and password are required" });

        if (!_users.TryGetValue(request.Username.ToLower(), out var user) || user.Password != request.Password)
            return Unauthorized(new { message = "Invalid credentials. Check the credentials in README file." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, request.Username),
            new("orgId", user.OrgId.ToString())
        };
        foreach (var role in user.Roles)
            claims.Add(new(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            username = request.Username,
            roles = user.Roles,
            orgId = user.OrgId
        });
    }

    [AllowAnonymous]
    [HttpGet("users")]
    public IActionResult GetUsers() =>
        Ok(_users.Select(u => new { username = u.Key, password = u.Value.Password, roles = u.Value.Roles, orgId = u.Value.OrgId }));
}
