using System.Net.Sockets;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

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
        // 1. วิเคราะห์ Exception ว่าเป็นปัญหาอะไรกันแน่
        var (statusCode, title, detail) = MapExceptionDetails(exception);

        // บันทึก Log พร้อมแยกระดับความรุนแรง
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "[Server Fault] {Title}: {Message}", title, exception.Message);
        }
        else
        {
            _logger.LogWarning("[Client Fault] {Title}: {Message}", title, exception.Message);
        }

        // 2. เช็กว่าเป็นคำขอจาก API หรือไม่
        bool isApiRequest = httpContext.Request.Path.StartsWithSegments("/api")
                         || httpContext.Request.Headers.Accept.ToString().Contains("application/json");

        if (!isApiRequest)
        {
            // ถ้าเป็น MVC หน้าเว็บ: คืนค่า false เพื่อให้ ExceptionHandlerMiddleware ใน Program.cs 
            // ส่งต่อไปยังหน้า ErrorController ผ่าน ReExecute โดยไม่หลุด Exception Data
            return false;
        }

        // 3. ฝั่ง Web API: ส่ง RFC 7807 ProblemDetails ที่ระบุรายละเอียดจริง
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapExceptionDetails(Exception ex) => ex switch
    {
        // ฐานข้อมูล / Supabase เชื่อมต่อไม่ได้ หรือคอนฟิกผิด
        NpgsqlException or SocketException => (
            StatusCodes.Status503ServiceUnavailable,
            "Database Connection Failed",
            "ไม่สามารถเชื่อมต่อไปยังฐานข้อมูลได้ในขณะนี้ กรุณาตรวจสอบ Network หรือ Connection String"
        ),

        // คอนฟิกระบบผิดพลาด (เช่น กรณี integrated security ที่เจอ)
        ArgumentException when ex.Source?.Contains("Npgsql") == true => (
            StatusCodes.Status500InternalServerError,
            "Database Configuration Error",
            "การตั้งค่าการเชื่อมต่อฐานข้อมูลไม่ถูกต้อง"
        ),

        // ปัญหาฝั่งผู้ใช้งานส่งค่าผิด (Validation ทั่วไป)
        ArgumentException or BadHttpRequestException => (
            StatusCodes.Status400BadRequest,
            "Bad Request",
            ex.Message
        ),

        // สิทธิ์การเข้าถึง
        UnauthorizedAccessException => (
            StatusCodes.Status401Unauthorized,
            "Unauthorized",
            "จำเป็นต้องยืนยันตัวตนก่อนเข้าถึงข้อมูล"
        ),

        // ค่าเริ่มต้น: Server พังแบบไม่คาดคิด
        _ => (
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "เกิดข้อผิดพลาดในการประมวลผลของระบบ ทีมงานกำลังเร่งตรวจสอบ"
        )
    };
}