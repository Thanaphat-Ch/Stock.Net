using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options; // ต้องใช้ตัวนี้เพื่อดึง IOptions
using MyApiProject.Models;

namespace MyApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestConfigController : ControllerBase
{
    // ตัวแปรภายใน Controller สำหรับเก็บค่าที่ส่งเข้ามา
    private readonly JwtSettings _jwtSettings;

    // Constructor Injection: .NET จะหยิบค่า JwtSettings จาก Program.cs ยัดเข้ามาให้ในพารามิเตอร์นี้อัตโนมัติ
    public TestConfigController(IOptions<JwtSettings> options)
    {
        // .Value คือการดึงค่า Object JwtSettings ตัวจริงออกมา
        Console.WriteLine("JwtSettings: {0}", options.Value);
        _jwtSettings = options.Value;
    }

    [HttpGet("check-config")]
    public IActionResult CheckConfig()
    {
        // ดึงตัวแปรที่เก็บไว้มาแสดงผล
        return Ok(new
        {
            IssuerName = _jwtSettings.Issuer,
            Minutes = _jwtSettings.ExpiryMinutes
        });
    }
}