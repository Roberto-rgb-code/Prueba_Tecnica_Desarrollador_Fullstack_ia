using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class AuthUserRepository : IAuthUserRepository
{
    private readonly AuthDbContext _context;

    public AuthUserRepository(AuthDbContext context) => _context = context;

    public Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(AuthUser user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
