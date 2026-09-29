using Serilog;

namespace MyApiProject.Extensions;

public static class LoggingExtensions
{
    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration) // อ่านค่าระดับ Log จาก appsettings.json
            .Enrich.FromLogContext()                       // เก็บ Context เพิ่มเติม เช่น TraceId, IP
            .WriteTo.Console()                             // 1. ส่งออกทางหน้าจอ Terminal
            .WriteTo.File(                                 // 2. บันทึกเก็บลงไฟล์
                path: "Logs/log-.txt",                     // ชื่อไฟล์ เช่น log-20260915.txt
                rollingInterval: RollingInterval.Day,      // ตัดไฟล์ใหม่ทุกๆ วันที่เปลี่ยนไป
                retainedFileCountLimit: 14                 // ลบไฟล์เก่าทิ้งอัตโนมัติหากเกิน 14 วัน
            )
            .CreateLogger();

        // สั่งให้ Host สลับตัวบันทึกข้อมูลหลักไปใช้ Serilog
        builder.Host.UseSerilog();
    }
}