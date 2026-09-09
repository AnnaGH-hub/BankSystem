using BankSystemPro.Domain.Enums;

namespace BankSystemPro.Application.DTOs
{
    public record RegisterRequest(string FullName, string Email, string Password, UserRole Role);

    public record LoginRequest(string Email, string Password);

    public record AuthResponse(int UserId, string FullName, string Email, string Role, string Token, DateTime ExpiresAt);
}
