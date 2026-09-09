using BankSystemPro.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BankSystemPro.Infrastructure.Security
{
    // Wraps ASP.NET Core Identity's PBKDF2-based hasher rather than rolling a custom
    // one - there's no good reason to hand-write password hashing in 2026.
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string password) => _hasher.HashPassword(new object(), password);

        public bool Verify(string password, string hash) =>
            _hasher.VerifyHashedPassword(new object(), hash, password) != PasswordVerificationResult.Failed;
    }
}
