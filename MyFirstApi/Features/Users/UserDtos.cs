namespace MyApiProject.Features.Users;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public record CreateUserDto(string Name, string Email, string PasswordHash, string Role);
public record UpdateUserDto(string Name, string Email, string PasswordHash, string Role);