using System.ComponentModel.DataAnnotations;

namespace MyApiProject.Models;

public class RegisterDto
{
    [Required(ErrorMessage = "กรุณากรอก Username")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username ต้องมีความยาว 3 ถึง 20 ตัวอักษร")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอก Email")]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอก Password")]
    [MinLength(6, ErrorMessage = "Password ต้องมีอย่างน้อย 6 ตัวอักษร")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "รหัสผ่านยืนยันไม่ตรงกับ Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(18, 100, ErrorMessage = "อายุต้องอยู่ระหว่าง 18 ถึง 100 ปี")]
    public int Age { get; set; }
}