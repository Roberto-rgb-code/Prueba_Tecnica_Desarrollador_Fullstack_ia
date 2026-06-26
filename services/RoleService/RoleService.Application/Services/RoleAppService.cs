using RoleService.Application.DTOs;
using RoleService.Application.Interfaces;
using RoleService.Domain.Entities;
using RoleService.Domain.Repositories;
using Toka.Shared.Events;
using Toka.Shared.Messaging;

namespace RoleService.Application.Services;

public class RoleAppService : IRoleService
{
    private readonly IRoleRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public RoleAppService(IRoleRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _repository.GetAllAsync(cancellationToken);
        return roles.Select(r => new RoleDto(r.Id, r.Name, r.Description, r.IsActive)).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _repository.GetByIdAsync(id, cancellationToken);
        return role is null ? null : new RoleDto(role.Id, role.Name, role.Description, role.IsActive);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Role name is required.");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsActive = request.IsActive
        };

        await _repository.AddAsync(role, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return new RoleDto(role.Id, role.Name, role.Description, role.IsActive);
    }

    public async Task<RoleDto?> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _repository.GetByIdAsync(id, cancellationToken);
        if (role is null) return null;

        role.Name = request.Name.Trim();
        role.Description = request.Description?.Trim() ?? string.Empty;
        role.IsActive = request.IsActive;

        await _repository.UpdateAsync(role, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return new RoleDto(role.Id, role.Name, role.Description, role.IsActive);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _repository.GetByIdAsync(id, cancellationToken);
        if (role is null) return false;

        await _repository.DeleteAsync(role, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AssignRoleResult?> AssignRoleAsync(Guid roleId, Guid userId, CancellationToken cancellationToken = default)
    {
        var role = await _repository.GetByIdAsync(roleId, cancellationToken);
        if (role is null) return null;

        if (!role.IsActive)
            throw new InvalidOperationException("Role is inactive.");

        if (await _repository.UserRoleExistsAsync(userId, roleId, cancellationToken))
            return new AssignRoleResult(userId, roleId, role.Name);

        await _repository.AssignUserRoleAsync(new UserRole { UserId = userId, RoleId = roleId }, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync("role.assigned", new RoleAssignedEvent(userId, roleId, role.Name, DateTime.UtcNow), cancellationToken);
        return new AssignRoleResult(userId, roleId, role.Name);
    }

    public async Task<IReadOnlyList<RoleDto>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roles = await _repository.GetRolesByUserIdAsync(userId, cancellationToken);
        return roles.Select(r => new RoleDto(r.Id, r.Name, r.Description, r.IsActive)).ToList();
    }
}
