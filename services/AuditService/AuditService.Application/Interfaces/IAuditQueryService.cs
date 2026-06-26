using AuditService.Application.DTOs;
using AuditService.Domain.Repositories;

namespace AuditService.Application.Interfaces;

public interface IAuditQueryService
{
    Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogDto>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default);
}

public class AuditQueryService : IAuditQueryService
{
    private readonly IAuditLogRepository _repository;

    public AuditQueryService(IAuditLogRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetRecentAsync(limit, cancellationToken);
        return logs.Select(AuditLogMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetByResourceAsync(resourceType, resourceId, cancellationToken);
        return logs.Select(AuditLogMapper.ToDto).ToList();
    }
}
