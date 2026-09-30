using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using MyApiProject.Endpoints;
using MyApiProject.Models;
using MyApiProject.Extensions;
using MyApiProject.Handlers;
using Serilog;
using MyApiProject.Features.Products;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SupabaseDb");

builder.AddSerilogLogging();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddJwtAuthentication(builder.Configuration); builder.Services.AddControllers();
builder.Services.AddCustomCors(builder.Configuration);

// ลงทะเบียนระบบ Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); // ช่วยจัดโครงสร้าง ProblemDetails

// DI Services
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // {1} คือชื่อ Feature/Controller เช่น Products
        // {0} คือชื่อ View/Action เช่น Index
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/Features/{1}/Views/{0}.cshtml");
        options.ViewLocationFormats.Add("/Features/{1}/{0}.cshtml"); // วาง Index.cshtml ไว้ข้าง Controller ได้เลย
        options.ViewLocationFormats.Add("/Features/Shared/{0}.cshtml");
    });
builder.Services.AddOpenApi();

// builder.Services.AddSingleton<IProductService, InMemoryProductRepository>();


var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStatusCodePagesWithRedirects("/error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles(); // เปิดให้เรียกไฟล์ static เช่น CSS, JS, รูปภาพ

app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");
app.MapGet("/", () => Results.Redirect("/products"));
// app.MapUserEndpoints();

try
{
    Log.Information("Starting Web API Application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start correctly");
}
finally
{
    Log.CloseAndFlush();
}

