namespace RoleService.Application.DTOs;

public record RoleDto(Guid Id, string Name, string Description, bool IsActive);
public record CreateRoleRequest(string Name, string Description, bool IsActive = true);
public record UpdateRoleRequest(string Name, string Description, bool IsActive);
public record AssignRoleResult(Guid UserId, Guid RoleId, string RoleName);
