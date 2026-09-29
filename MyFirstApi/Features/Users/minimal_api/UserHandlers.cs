using Microsoft.EntityFrameworkCore;
using MyApiProject.Models;
using MyApiProject.Features.Users;

namespace MyApiProject.Handlers;

public static class UserHandlers
{
    public static async Task<IResult> GetAllUsers(AppDbContext db)
    {
        var users = await db.Users2.ToListAsync();
        return Results.Ok(users);
    }

    public static async Task<IResult> GetUserById(int id, AppDbContext db)
    {
        var user = await db.Users2.FindAsync(id);
        return user is not null ? Results.Ok(user) : Results.NotFound(new { message = $"User with ID {id} not found" });
    }

    public static async Task<IResult> CreateUser(CreateUserDto dto, AppDbContext db)
    {
        var user = new User { Name = dto.Name, Email = dto.Email };
        db.Users2.Add(user);
        await db.SaveChangesAsync();
        return Results.Created($"/api/users/{user.Id}", user);
    }

    public static async Task<IResult> UpdateUser(int id, UpdateUserDto dto, AppDbContext db)
    {
        var user = await db.Users2.FindAsync(id);
        if (user is null) return Results.NotFound(new { message = $"User with ID {id} not found" });

        user.Name = dto.Name;
        user.Email = dto.Email;
        await db.SaveChangesAsync();
        return Results.Ok(user);
    }

    public static async Task<IResult> DeleteUser(int id, AppDbContext db)
    {
        var user = await db.Users2.FindAsync(id);
        if (user is null) return Results.NotFound(new { message = $"User with ID {id} not found" });

        db.Users2.Remove(user);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}