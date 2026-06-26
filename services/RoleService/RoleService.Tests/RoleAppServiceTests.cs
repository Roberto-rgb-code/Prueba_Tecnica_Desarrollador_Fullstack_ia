using RoleService.Application.DTOs;
using RoleService.Application.Services;
using RoleService.Domain.Entities;
using RoleService.Domain.Repositories;
using Moq;
using Toka.Shared.Messaging;

namespace RoleService.Tests;

public class RoleAppServiceTests
{
    private readonly Mock<IRoleRepository> _repository = new();
    private readonly Mock<IEventPublisher> _publisher = new();
    private readonly RoleAppService _service;

    public RoleAppServiceTests()
    {
        _service = new RoleAppService(_repository.Object, _publisher.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_ReturnsRole()
    {
        _repository.Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(new CreateRoleRequest("Manager", "Team manager"));

        Assert.Equal("Manager", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(new CreateRoleRequest("", "desc")));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRoles()
    {
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { new() { Name = "Admin", Description = "Full", IsActive = true } });

        var result = await _service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Admin", result[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsRole()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = id, Name = "User", Description = "Standard", IsActive = true });

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal("User", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        Assert.Null(await _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_UpdatesRole()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = id, Name = "Old", Description = "Old desc", IsActive = true });
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(id, new UpdateRoleRequest("New", "New desc", false));

        Assert.NotNull(result);
        Assert.Equal("New", result!.Name);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = id, Name = "Temp" });
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        Assert.True(await _service.DeleteAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AssignRoleAsync_WhenRoleNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

        var result = await _service.AssignRoleAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AssignRoleAsync_WhenRoleInactive_Throws()
    {
        var roleId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = roleId, Name = "Disabled", IsActive = false });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AssignRoleAsync(roleId, Guid.NewGuid()));
    }

    [Fact]
    public async Task AssignRoleAsync_WhenNewAssignment_PublishesEvent()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = roleId, Name = "Admin", IsActive = true });
        _repository.Setup(r => r.UserRoleExistsAsync(userId, roleId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.AssignRoleAsync(roleId, userId);

        Assert.NotNull(result);
        _publisher.Verify(p => p.PublishAsync("role.assigned", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_WhenAlreadyAssigned_SkipsPublish()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = roleId, Name = "Admin", IsActive = true });
        _repository.Setup(r => r.UserRoleExistsAsync(userId, roleId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _service.AssignRoleAsync(roleId, userId);

        Assert.NotNull(result);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserRolesAsync_ReturnsMappedRoles()
    {
        var userId = Guid.NewGuid();
        _repository.Setup(r => r.GetRolesByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { new() { Name = "User", Description = "Standard", IsActive = true } });

        var result = await _service.GetUserRolesAsync(userId);

        Assert.Single(result);
        Assert.Equal("User", result[0].Name);
    }
}
