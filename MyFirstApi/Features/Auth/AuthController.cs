using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyApiApp.Models;
using MyApiProject.Models;

namespace MyApiApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    // รับค่า config เข้ามาใช้งาน
    public AuthController(IOptions<JwtSettings> options)
    {
        _jwtSettings = options.Value;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        // 1. ตรวจสอบ Username / Password (สมมติข้อมูลจำลอง)
        if (dto.Username != "admin" || dto.Password != "123456")
        {
            return Unauthorized(new { message = "Username หรือ Password ไม่ถูกต้อง" }); // Status 401
        }

        // 2. ข้อมูลที่จะฝังลงใน Token (เรียกว่า Claims)
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, dto.Username),
            new Claim(ClaimTypes.Role, "Admin") // ฝังยศ Admin เข้าไป
        };

        // 3. นำ SecretKey มาเข้ารหัส
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. บรรจุกล่อง Token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes), // วันหมดอายุ
            signingCredentials: creds
        );

        // 5. แปลง Object Token ให้ออกมาเป็นข้อความ String ยาวๆ
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = tokenString });
    }

    // --- เส้นทดสอบการเช็คสิทธิ์ (Authorization) ---
    [HttpGet("admin-data")]
    [Authorize(Roles = "Admin")] // ใส่ Attribute นี้ = ใครไม่มี Token หรือไม่ใช่ Admin จะเข้าไม่ได้!
    public IActionResult GetAdminSecret()
    {
        // ดึงชื่อคนที่เรียกมาจาก Token ได้เลย
        var currentUser = User.Identity?.Name;
        return Ok(new { message = $"ยินดีต้อนรับคุณ {currentUser}! นี่คือข้อมูลลับระดับ Admin" });
    }
}