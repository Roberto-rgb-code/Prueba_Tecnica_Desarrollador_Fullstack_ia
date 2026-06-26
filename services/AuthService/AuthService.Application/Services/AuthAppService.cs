using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using Microsoft.Extensions.Options;
using Toka.Shared.Auth;
using Toka.Shared.Events;
using Toka.Shared.Messaging;

namespace AuthService.Application.Services;

public class AuthAppService : IAuthService
{
    private readonly IAuthUserRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly JwtSettings _jwtSettings;

    public AuthAppService(IAuthUserRepository repository, IEventPublisher eventPublisher, IOptions<JwtSettings> jwtSettings)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCredentials(request.Email, request.Password);

        if (await _repository.GetByEmailAsync(request.Email, cancellationToken) is not null)
            throw new InvalidOperationException("Email already registered.");

        var user = new AuthUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName.Trim()
        };

        await _repository.AddAsync(user, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync("user.created", new UserCreatedEvent(user.Id, user.Email, user.FullName, DateTime.UtcNow), cancellationToken);
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        await _eventPublisher.PublishAsync("user.logged_in", new UserLoggedInEvent(user.Id, user.Email, DateTime.UtcNow), cancellationToken);
        return BuildAuthResponse(user);
    }

    public async Task<UserInfoDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : new UserInfoDto(user.Id, user.Email, user.FullName);
    }

    private AuthResponse BuildAuthResponse(AuthUser user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        var token = GenerateToken(user, expires);
        return new AuthResponse(token, user.Id, user.Email, user.FullName, expires);
    }

    private string GenerateToken(AuthUser user, DateTime expires)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwtSettings.Issuer, _jwtSettings.Audience, claims, expires: expires, signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void ValidateCredentials(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Invalid email.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters.");
    }
}
