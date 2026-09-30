namespace MyApiProject.Extensions;

public static class CorsExtensions
{
    // กำหนดชื่อ Policy เป็นค่าคงที่ ป้องกันการพิมพ์ผิด
    public const string FrontendPolicy = "FrontendPolicy";

    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        // ดึง Origins ที่อนุญาตจาก appsettings.json (เพื่อความยืดหยุ่นตอน Deploy)
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
                             ?? new[] { "http://localhost:3000", "http://localhost:5173" };

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)  // 1. อนุญาตเฉพาะ Domain/Port เหล่านี้
                      .AllowAnyMethod()             // 2. อนุญาตทุก Method (GET, POST, PUT, DELETE ฯลฯ)
                      .AllowAnyHeader()             // 3. อนุญาตทุก Header (รวมถึง Authorization, Content-Type)
                      .AllowCredentials();          // 4. (ทางเลือก) อนุญาตให้ส่ง Cookies หรือ Auth Headers ข้าม Origin ได้
            });
        });

        return services;
    }
}