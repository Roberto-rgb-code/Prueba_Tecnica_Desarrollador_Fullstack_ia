using AuditService.Domain.Entities;

namespace AuditService.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default);
}
