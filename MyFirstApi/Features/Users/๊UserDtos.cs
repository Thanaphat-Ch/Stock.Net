namespace MyApiProject.Features.Users;

public record CreateUserDto(string Name, string Email);
public record UpdateUserDto(string Name, string Email);