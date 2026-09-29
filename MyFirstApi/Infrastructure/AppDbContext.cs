using Microsoft.EntityFrameworkCore;
using MyApiProject.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users2 => Set<User>();
    public DbSet<Product> Products => Set<Product>();
}

//dotnet tool install --global dotnet-ef
//dotnet ef migrations add InitialCreate -o 
//dotnet ef database update

// # 1. สร้างสคริปต์ Migration ใหม่ (ตั้งชื่อสื่อความหมาย เช่น AddProductTable)
// dotnet ef migrations add AddProductTable

// # 2. ยิงคำสั่งไปสร้างตารางจริงใน Postgres/Supabase ทันที
// dotnet ef database update