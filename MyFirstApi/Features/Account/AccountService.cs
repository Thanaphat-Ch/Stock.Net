using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApiProject.Models;
using MyFirstApi.Models;

namespace MyApiProject.Features.Account;

public class AccountService : IAccountService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountService(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<(bool Success, string? ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model)
    {
        var emailExists = await _db.Users2.AnyAsync(u => u.Email == model.Email);
        if (emailExists) return (false, "อีเมลนี้ถูกใช้งานแล้วในระบบ", null);

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Role = "Staff",
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _db.Users2.Add(user);
        await _db.SaveChangesAsync();
        return (true, null, user);
    }

    public async Task<(bool Success, string? ErrorMessage, User? User)> ValidateLoginAsync(LoginViewModel model)
    {
        var user = await _db.Users2.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null) return (false, "อีเมลหรือรหัสผ่านไม่ถูกต้อง", null);
     
        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verifyResult == PasswordVerificationResult.Failed) return (false, "อีเมลหรือรหัสผ่านไม่ถูกต้อง", null);

        return (true, null, user);
    }
}