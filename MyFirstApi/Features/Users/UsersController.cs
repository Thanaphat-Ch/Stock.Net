// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiProject.Models;
using MyApiProject.Features.Users;
using Microsoft.AspNetCore.Authorization;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users2.ToListAsync();
        return Ok(users);
    }

    // GET: /api/users/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _db.Users2.FindAsync(id);
        return user is not null ? Ok(user) : NotFound();
    }

    // POST: /api/users
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = new User { Name = dto.Name, Email = dto.Email };
        _db.Users2.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    // DELETE: /api/users/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users2.FindAsync(id);
        if (user is null) return NotFound();

        _db.Users2.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }


    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        return Ok(new
        {
            message = "ข้อมูลถูกต้อง สมัครสมาชิกสำเร็จ",
            username = dto.Username,
            email = dto.Email
        });
    }
}