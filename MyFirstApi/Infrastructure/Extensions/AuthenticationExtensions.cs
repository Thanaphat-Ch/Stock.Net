using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MyApiProject.Models;
using System.Text.Json;

namespace MyApiProject.Extensions;

public static class AuthenticationExtensions
{
    // สร้าง Extension Method สำหรับ IServiceCollection
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. อ่านค่าจาก appsettings.json
        var jwtSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSection);
        
        var jwtSettings = jwtSection.Get<JwtSettings>();
        if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JwtSettings:SecretKey is not configured in appsettings.json");
        }

        // 2. ตั้งค่าระบบตรวจ Token
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                )
            };
            // ดักจับ Event เมื่อเกิดการปฏิเสธคำขอ
            options.Events = new JwtBearerEvents
            {
                // 1. ดักจับเมื่อไม่มี Token หรือ Token ไม่ถูกต้อง / หมดอายุ (HTTP 401)
                OnChallenge = async context =>
                {
                    // สั่งหยุดการทำงานเริ่มต้นของ Framework
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = 401,
                        title = "Unauthorized",
                        message = "คุณยังไม่ได้เข้าสู่ระบบ หรือ Token หมดอายุแล้ว กรุณาแนบ Bearer Token ที่ถูกต้อง"
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                },

                // 2. ดักจับเมื่อมี Token แต่ไม่มีสิทธิ์ / Role ไม่ถึง (HTTP 403)
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = 403,
                        title = "Forbidden",
                        message = "คุณไม่มีสิทธิ์เข้าถึงฟังก์ชันนี้ (สิทธิ์ไม่เพียงพอ)"
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}