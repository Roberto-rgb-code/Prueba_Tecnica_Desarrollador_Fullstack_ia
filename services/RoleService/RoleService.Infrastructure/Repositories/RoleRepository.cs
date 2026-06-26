using RoleService.Domain.Entities;
using RoleService.Domain.Repositories;
using RoleService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RoleService.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly RoleDbContext _context;

    public RoleRepository(RoleDbContext context) => _context = context;

    public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Role>)t.Result, cancellationToken);

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _context.Roles.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default) =>
        await _context.Roles.AddAsync(role, cancellationToken);

    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(role);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Remove(role);
        return Task.CompletedTask;
    }

    public Task<bool> UserRoleExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default) =>
        _context.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);

    public async Task AssignUserRoleAsync(UserRole userRole, CancellationToken cancellationToken = default) =>
        await _context.UserRoles.AddAsync(userRole, cancellationToken);

    public async Task<IReadOnlyList<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
