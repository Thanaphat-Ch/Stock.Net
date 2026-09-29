using Microsoft.AspNetCore.Mvc;

namespace MyApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestErrorController : ControllerBase
{
    [HttpGet("crash-500")]
    public IActionResult CrashSystem()
    {
        // จำลองสถานการณ์ระบบพังที่ไม่คาดคิด เช่น NullReferenceException หรือ Database ล่ม
        throw new Exception("เชื่อมต่อ Database ไม่ได้ หรือ Object เป็น Null");
    }

    [HttpGet("bad-input-400")]
    public IActionResult BadInput()
    {
        Console.WriteLine("Simulating a 400 error...");
        // จำลองสถานการณ์ส่ง Argument ผิด
        throw new ArgumentException("ค่าพารามิเตอร์ไม่ถูกต้องตามเงื่อนไข");
    }
}