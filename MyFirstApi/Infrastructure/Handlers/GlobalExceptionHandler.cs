using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MyApiProject.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. บันทึก Log ทุกกรณี (ทั้งหน้าเว็บและ API)
        _logger.LogError(exception, "เกิดข้อผิดพลาดที่ไม่ได้จัดการ: {Message}", exception.Message);

        // 2. เช็กว่าเป็นคำขอจากฝั่ง Web API หรือไม่?
        bool isApiRequest = httpContext.Request.Path.StartsWithSegments("/api")
                         || httpContext.Request.Headers.Accept.ToString().Contains("application/json");

        if (!isApiRequest)
        {
            // ถ้าเป็นคำขอเปิดหน้าเว็บทั่วไป (MVC):
            // สั่ง Redirect ไปยังหน้า UI Error พร้อมระบุ StatusCode
            int statusCode = exception is ArgumentException 
                ? StatusCodes.Status400BadRequest 
                : StatusCodes.Status500InternalServerError;

            httpContext.Response.Redirect($"/error/{statusCode}");
            
            // คืนค่า true เพื่อจบขั้นตอน ไม่ต้องพ่น JSON ออกไป
            return true;
        }

        // 3. ถ้าเป็นคำขอฝั่ง Web API ให้ส่ง JSON ProblemDetails ตามปกติ
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Detail = "เกิดข้อผิดพลาดภายในระบบ กรุณาลองใหม่อีกครั้งในภายหลัง",
            Instance = httpContext.Request.Path
        };

        if (exception is ArgumentException)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Bad Request";
            problemDetails.Detail = exception.Message;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; 
    }
}