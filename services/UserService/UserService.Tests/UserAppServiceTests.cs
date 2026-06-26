using UserService.Application.DTOs;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using Moq;
using Toka.Shared.Messaging;

namespace UserService.Tests;

public class UserAppServiceTests
{
    private readonly Mock<IUserRepository> _repository = new();
    private readonly Mock<IEventPublisher> _publisher = new();
    private readonly Mock<IUserCache> _cache = new();
    private readonly UserAppService _service;

    public UserAppServiceTests()
    {
        _service = new UserAppService(_repository.Object, _publisher.Object, _cache.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsUser()
    {
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(new CreateUserRequest("test@toka.com", "John", "Doe"));

        Assert.Equal("test@toka.com", result.Email);
        Assert.Equal("John Doe", result.FullName);
        _cache.Verify(c => c.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidEmail_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(new CreateUserRequest("invalid", "John", "Doe")));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_Throws()
    {
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "dup@toka.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(new CreateUserRequest("dup@toka.com", "John", "Doe")));
    }

    [Fact]
    public async Task CreateAsync_WithMissingName_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(new CreateUserRequest("user@toka.com", "", "Doe")));
    }

    [Fact]
    public async Task GetAllAsync_UsesCacheWhenAvailable()
    {
        var cached = new List<UserDto> { new(Guid.NewGuid(), "a@toka.com", "A", "B", "A B", true, DateTime.UtcNow) };
        _cache.Setup(c => c.GetListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(cached);

        var result = await _service.GetAllAsync();

        Assert.Single(result);
        _repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_LoadsFromRepositoryWhenCacheMiss()
    {
        _cache.Setup(c => c.GetListAsync(It.IsAny<CancellationToken>())).ReturnsAsync((IReadOnlyList<UserDto>?)null);
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { new() { Email = "u@toka.com", FirstName = "U", LastName = "Ser" } });

        var result = await _service.GetAllAsync();

        Assert.Single(result);
        _cache.Verify(c => c.SetListAsync(It.IsAny<IReadOnlyList<UserDto>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsUser()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = id, Email = "u@toka.com", FirstName = "U", LastName = "Ser" });

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal("u@toka.com", result!.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Assert.Null(await _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesAndReturns()
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, Email = "u@toka.com", FirstName = "Old", LastName = "Name", IsActive = true };
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(id, new UpdateUserRequest("New", "Name", false));

        Assert.NotNull(result);
        Assert.False(result!.IsActive);
        Assert.Equal("New Name", result.FullName);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserMissing_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Assert.Null(await _service.UpdateAsync(Guid.NewGuid(), new UpdateUserRequest("A", "B", true)));
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = id });
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var deleted = await _service.DeleteAsync(id);

        Assert.True(deleted);
        _cache.Verify(c => c.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserMissing_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));
    }
}
