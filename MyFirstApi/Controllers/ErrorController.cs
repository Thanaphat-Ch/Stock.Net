using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MyApiProject.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    private readonly IWebHostEnvironment _env;

    public ErrorController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [Route("")]
    [Route("{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index(int? statusCode = null)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        // ดึง Exception ดั้งเดิมที่เกิดขึ้นก่อนหน้า
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionFeature?.Error;

        var model = new ErrorViewModel
        {
            StatusCode = statusCode ?? (exception != null ? 500 : Response.StatusCode),
            RequestId = requestId,
            OriginalPath = exceptionFeature?.Path ?? HttpContext.Request.Path
        };

        // หากมี Exception จริง ให้อ่านสาเหตุเจาะจง
        if (exception != null)
        {
            ApplyExceptionDetails(model, exception);
        }
        else
        {
            // หากไม่มี Exception แต่เป็น HTTP Status Code เปล่าๆ (เช่น 404, 401)
            ApplyStatusCodeDetails(model);
        }

        Response.StatusCode = model.StatusCode;
        return View("~/Features/Shared/Error.cshtml", model);
    }

    private void ApplyExceptionDetails(ErrorViewModel model, Exception ex)
    {
        switch (ex)
        {
            case NpgsqlException or System.Net.Sockets.SocketException:
                model.StatusCode = 503;
                model.Title = "ไม่สามารถเชื่อมต่อฐานข้อมูลได้ (503)";
                model.Description = "ระบบไม่สามารถติดต่อฐานข้อมูล Supabase ได้ กรุณาตรวจสอบสถานะเซิร์ฟเวอร์หรือเครือข่าย";
                break;

            case ArgumentException when ex.Source?.Contains("Npgsql") == true:
                model.StatusCode = 500;
                model.Title = "ข้อผิดพลาดคอนฟิกฐานข้อมูล (500)";
                model.Description = "Connection String ของระบบระบุพารามิเตอร์ไม่ถูกต้อง";
                break;

            default:
                model.StatusCode = 500;
                model.Title = "ระบบขัดข้องภายใน (500)";
                model.Description = "เกิดข้อผิดพลาดในการประมวลผลข้อมูล";
                break;
        }

        // แสดงรายละเอียด Exception เชิงลึกเฉพาะช่วง Dev เพื่อความปลอดภัย
        if (_env.IsDevelopment())
        {
            model.TechnicalDetails = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
        }
    }

    private static void ApplyStatusCodeDetails(ErrorViewModel model)
    {
        (model.Title, model.Description) = model.StatusCode switch
        {
            400 => ("คำขอไม่ถูกต้อง (400)", "รูปแบบข้อมูลที่ส่งมาไม่ถูกต้อง"),
            401 => ("กรุณาเข้าสู่ระบบ (401)", "คุณยังไม่ได้เข้าสู่ระบบ หรือเซสชันหมดอายุ"),
            403 => ("ไม่มีสิทธิ์เข้าถึง (403)", "คุณไม่มีสิทธิ์ใช้งานในส่วนนี้"),
            404 => ("ไม่พบหน้าที่ต้องการ (404)", "URL ที่ระบุไม่ถูกต้องหรือไม่มีอยู่อีกต่อไป"),
            405 => ("Method ไม่ถูกต้อง (405)", "HTTP Method ไม่ได้รับอนุญาตสำหรับหน้านี้"),
            503 => ("ระบบปิดปรับปรุงชั่วคราว (503)", "เซิร์ฟเวอร์ปลายทางยังไม่พร้อมให้บริการ"),
            _ => ($"เกิดข้อผิดพลาด ({model.StatusCode})", "ระบบพบปัญหาที่ไม่คาดคิด")
        };
    }
}

public class ErrorViewModel
{
    public int StatusCode { get; set; } = 500;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string OriginalPath { get; set; } = string.Empty;
    public string? TechnicalDetails { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}