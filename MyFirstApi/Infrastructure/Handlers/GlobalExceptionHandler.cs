using System.Net.Sockets;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MyApiProject.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. ตรวจสอบว่า Response เริ่มส่งออกไปหา Client หรือยัง
        if (httpContext.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, skipping GlobalExceptionHandler.");
            return false;
        }

        // 2. วิเคราะห์ Exception (คลี่ดู InnerException ด้วย)
        var (statusCode, title, detail) = MapExceptionDetails(exception);

        // บันทึก Log ตามระดับความรุนแรง
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "[Server Fault] {Title}: {Message}", title, exception.Message);
        }
        else
        {
            _logger.LogWarning("[Client Fault] {Title}: {Message}", title, exception.Message);
        }

        // 3. เช็กว่าเป็นคำขอจาก API หรือไม่
        bool isApiRequest = httpContext.Request.Path.StartsWithSegments("/api")
                         || httpContext.Request.Headers.Accept.ToString().Contains("application/json");

        if (!isApiRequest)
        {
            // ถ้าเป็น MVC หน้าเว็บ: คืน false ให้ UseExceptionHandler("/error") นำไปแสดงหน้า Error View
            return false;
        }

        // 4. ฝั่ง Web API: คืน ProblemDetails JSON
        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            },
            Exception = exception
        });
    }




    private static (int StatusCode, string Title, string Detail) MapExceptionDetails(Exception ex)
    {
        var rootCause = GetInnermostException(ex);

        return rootCause switch
        {
            // ตรวจ Unique Violation ก่อน (เพราะ PostgresException สืบทอดมาจาก NpgsqlException)
            PostgresException pgEx when pgEx.SqlState == PostgresErrorCodes.UniqueViolation => (
                StatusCodes.Status409Conflict,
                "Duplicate Entry",
                "มีข้อมูลรายการนี้อยู่ในระบบแล้ว ไม่สามารถบันทึกซ้ำได้"
            ),

            // ตรวจ Connection/Network Error ทั่วไปของ Npgsql และ Socket
            NpgsqlException or SocketException or TimeoutException => (
                StatusCodes.Status503ServiceUnavailable,
                "Database Connection Failed",
                "ไม่สามารถเชื่อมต่อไปยังฐานข้อมูลได้ในขณะนี้ กรุณาตรวจสอบ Network หรือการตั้งค่า Database"
            ),

            // ตรวจ ArgumentException เฉพาะเคส Npgsql ก่อน
            ArgumentException argEx when argEx.Source?.Contains("Npgsql") == true => (
                StatusCodes.Status500InternalServerError,
                "Database Configuration Error",
                "การตั้งค่าการเชื่อมต่อฐานข้อมูลไม่ถูกต้อง"
            ),

            // ค่อยมาตรวจ ArgumentException / Bad Request ทั่วไป
            ArgumentException or BadHttpRequestException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                rootCause.Message
            ),

            // ตรวจ Auth
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "จำเป็นต้องยืนยันตัวตนก่อนเข้าถึงข้อมูล"
            ),

            // Default Fallback อยู่ล่างสุดเสมอ
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "เกิดข้อผิดพลาดในการประมวลผลของระบบ ทีมงานกำลังเร่งตรวจสอบ"
            )
        };
    }

    private static Exception GetInnermostException(Exception ex)
    {
        var current = ex;
        while (current.InnerException != null)
        {
            current = current.InnerException;
        }
        return current;
    }
}