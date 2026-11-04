using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiProject.Features.Users;
using MyApiProject.Models;
using MyFirstApi.Models;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    // GET: /api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users2
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: /api/users/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _db.Users2.FindAsync(id);
        if (user is null) return NotFound();

        var response = new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(response);
    }

    // POST: /api/users/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var emailExists = await _db.Users2.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            return BadRequest(new { message = "อีเมลนี้ถูกใช้งานแล้ว" });
        }

        var user = new User
        {
            Name = dto.Username,
            Email = dto.Email,
            Role = "Staff",
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _db.Users2.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        });
    }

    // DELETE: /api/users/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users2.FindAsync(id);
        if (user is null) return NotFound();

        _db.Users2.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}