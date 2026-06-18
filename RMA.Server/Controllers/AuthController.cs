using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using RMA.Server.Entities;
using RMA.Server.Services;
using Microsoft.Extensions.Configuration;

namespace RMA.Server.Controllers;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FirestoreRepository<UserAccount> _userRepo;
    private readonly IConfiguration _config;

    public AuthController(FirestoreRepository<UserAccount> userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Tên đăng nhập và mật khẩu là bắt buộc." });
        }

        // 1. Kiểm tra tài khoản Super Admin từ appsettings.json
        var superAdminUser = _config["SuperAdmin:Username"] ?? "admin";
        var superAdminPass = _config["SuperAdmin:Password"] ?? "admin123";

        if (request.Username.Equals(superAdminUser, StringComparison.OrdinalIgnoreCase) && request.Password == superAdminPass)
        {
            var token = GenerateJwtToken(superAdminUser, "Admin");
            return Ok(new
            {
                token = token,
                role = "Admin",
                message = "Đăng nhập thành công với quyền Super Admin."
            });
        }

        // 2. Kiểm tra tài khoản từ database Firestore
        try
        {
            var users = await _userRepo.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                if (user.Status != "Active")
                {
                    return BadRequest(new { message = "Tài khoản này đã bị vô hiệu hóa." });
                }

                // Verify mật khẩu sử dụng BCrypt
                bool passwordValid = false;
                try
                {
                    passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                }
                catch
                {
                    // Fallback trong trường hợp dữ liệu cũ hoặc test chưa được hash
                    if (request.Password == user.PasswordHash)
                    {
                        passwordValid = true;
                    }
                }

                if (passwordValid)
                {
                    var token = GenerateJwtToken(user.Username, user.Role);
                    return Ok(new
                    {
                        token = token,
                        role = user.Role,
                        message = $"Đăng nhập thành công với vai trò {user.Role}."
                    });
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi hệ thống khi đăng nhập: {ex.Message}" });
        }

        return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });
    }

    [HttpGet("test")]
    [Authorize]
    public IActionResult TestAuth()
    {
        var userName = User.Identity?.Name ?? "Unknown";
        return Ok(new { message = $"Bạn đã truy cập thành công! User: {userName}" });
    }

    private string GenerateJwtToken(string username, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("RMA_SongLinh_SecretKey_For_Local_Testing_Only_12345");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(4),
            Issuer = "RMAServer",
            Audience = "RMAServer",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
