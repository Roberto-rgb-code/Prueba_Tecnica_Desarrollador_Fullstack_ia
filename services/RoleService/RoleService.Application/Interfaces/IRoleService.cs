using RoleService.Application.DTOs;

namespace RoleService.Application.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleDto?> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AssignRoleResult?> AssignRoleAsync(Guid roleId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleDto>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}
