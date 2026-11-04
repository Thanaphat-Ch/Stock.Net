using MyApiProject.Models;
using MyFirstApi.Models;

namespace MyApiProject.Features.Account;

public interface IAccountService
{
    Task<(bool Success, string? ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model);
    Task<(bool Success, string? ErrorMessage, User? User)> ValidateLoginAsync(LoginViewModel model);
}