using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = "Staff";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}