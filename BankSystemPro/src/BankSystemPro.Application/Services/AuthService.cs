using BankSystemPro.Application.Common;
using BankSystemPro.Application.DTOs;
using BankSystemPro.Application.Interfaces;
using BankSystemPro.Domain.Entities;

namespace BankSystemPro.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokenService;

        public AuthService(IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokenService)
        {
            _uow = uow;
            _hasher = hasher;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existing = await _uow.Users.GetByEmailAsync(request.Email);
            if (existing is not null)
                throw new BadRequestException("A user with this email already exists.");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = _hasher.Hash(request.Password),
                Role = request.Role
            };

            await _uow.Users.AddAsync(user);
            await _uow.SaveChangesAsync();

            var (token, expiresAt) = _tokenService.GenerateToken(user);
            return new AuthResponse(user.Id, user.FullName, user.Email, user.Role.ToString(), token, expiresAt);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _uow.Users.GetByEmailAsync(request.Email);
            if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
                throw new BadRequestException("Invalid email or password.");

            var (token, expiresAt) = _tokenService.GenerateToken(user);
            return new AuthResponse(user.Id, user.FullName, user.Email, user.Role.ToString(), token, expiresAt);
        }
    }
}
