using AuthService.Application.DTOs;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Toka.Shared.Auth;
using Toka.Shared.Messaging;

namespace AuthService.Tests;

public class AuthAppServiceTests
{
    private readonly Mock<IAuthUserRepository> _repository = new();
    private readonly Mock<IEventPublisher> _publisher = new();
    private readonly AuthAppService _service;

    public AuthAppServiceTests()
    {
        var jwt = Options.Create(new JwtSettings
        {
            Issuer = "test",
            Audience = "test",
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposes123!",
            ExpirationMinutes = 60
        });
        _service = new AuthAppService(_repository.Object, _publisher.Object, jwt);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsToken()
    {
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((AuthUser?)null);
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.RegisterAsync(new RegisterRequest("test@toka.com", "password123", "Test User"));

        Assert.NotEmpty(result.Token);
        Assert.Equal("test@toka.com", result.Email);
        _repository.Verify(r => r.AddAsync(It.IsAny<AuthUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.PublishAsync("user.created", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = new AuthUser
        {
            Id = Guid.NewGuid(),
            Email = "test@toka.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            FullName = "Test User"
        };
        _repository.Setup(r => r.GetByEmailAsync("test@toka.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginRequest("test@toka.com", "password123"));

        Assert.NotEmpty(result.Token);
        Assert.Equal(user.Id, result.UserId);
        _publisher.Verify(p => p.PublishAsync("user.logged_in", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_Throws()
    {
        var user = new AuthUser { Email = "test@toka.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") };
        _repository.Setup(r => r.GetByEmailAsync("test@toka.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(new LoginRequest("test@toka.com", "wrong")));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_Throws()
    {
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((AuthUser?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(new LoginRequest("missing@toka.com", "password123")));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_Throws()
    {
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthUser { Email = "dup@toka.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterAsync(new RegisterRequest("dup@toka.com", "password123", "Dup")));
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidEmail_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync(new RegisterRequest("invalid", "password123", "User")));
    }

    [Fact]
    public async Task RegisterAsync_WithShortPassword_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync(new RegisterRequest("user@toka.com", "12345", "User")));
    }

    [Fact]
    public async Task GetUserAsync_WhenUserExists_ReturnsDto()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthUser { Id = id, Email = "user@toka.com", FullName = "User Test" });

        var result = await _service.GetUserAsync(id);

        Assert.NotNull(result);
        Assert.Equal("user@toka.com", result!.Email);
    }

    [Fact]
    public async Task GetUserAsync_WhenUserMissing_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthUser?)null);

        var result = await _service.GetUserAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
