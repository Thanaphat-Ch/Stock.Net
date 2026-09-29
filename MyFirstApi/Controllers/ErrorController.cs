using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MyApiProject.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    // ใช้ [Route] โดยไม่ระบุ [HttpGet] เพื่อให้รองรับทุก HTTP Method (GET, POST, PUT, DELETE)
    // ป้องกันการชน 405 ซ้ำตอนที่ Middleware ReExecute ด้วย Method เดิมเข้ามา
    [Route("")]
    [Route("{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index(int? statusCode = null)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var model = new ErrorViewModel
        {
            StatusCode = statusCode ?? Response.StatusCode,
            RequestId = requestId
        };

        switch (model.StatusCode)
        {
            case 404:
                model.Title = "ไม่พบหน้าที่คุณต้องการ (404)";
                model.Description = "หน้าที่คุณกำลังค้นหาอาจถูกย้าย ลบ หรือ URL ไม่ถูกต้อง";
                break;

            case 405:
                model.Title = "Method Not Allowed (405)";
                model.Description = "รูปแบบการเรียกใช้งาน (HTTP Method) ไม่ได้รับอนุญาตสำหรับหน้านี้";
                break;

            case 400:
                model.Title = "คำขอไม่ถูกต้อง (400)";
                model.Description = "ข้อมูลที่ส่งมามีรูปแบบไม่ถูกต้องหรือไม่ครบถ้วน";
                break;

            default:
                model.Title = $"เกิดข้อผิดพลาด ({model.StatusCode})";
                model.Description = "ระบบพบปัญหาที่ไม่คาดคิด ทีมงานกำลังเร่งตรวจสอบข้อมูล";
                break;
        }

        return View("~/Features/Shared/Error.cshtml", model);
    }
}

public class ErrorViewModel
{
    public int StatusCode { get; set; } = 500;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}