using Microsoft.EntityFrameworkCore;
using MyApiProject.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users2 => Set<User>();
    public DbSet<Product> Products2 => Set<Product>();
}

