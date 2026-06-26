using AuditService.Domain.Entities;
using AuditService.Domain.Repositories;

namespace AuditService.Infrastructure.Persistence;

public class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _logs = [];

    public Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        _logs.Add(log);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLog>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AuditLog>>(_logs.OrderByDescending(x => x.OccurredAtUtc).Take(limit).ToList());

    public Task<IReadOnlyList<AuditLog>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AuditLog>>(
            _logs.Where(x => x.ResourceType == resourceType && x.ResourceId == resourceId)
                .OrderByDescending(x => x.OccurredAtUtc).ToList());
}
