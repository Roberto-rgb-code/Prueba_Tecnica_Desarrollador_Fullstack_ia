using AuditService.Domain.Entities;
using AuditService.Domain.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AuditService.Infrastructure.Persistence;

public class MongoAuditSettings
{
    public const string SectionName = "MongoDb";
    public string ConnectionString { get; set; } = "mongodb://mongodb:27017";
    public string DatabaseName { get; set; } = "TokaAudit";
    public string CollectionName { get; set; } = "audit_logs";
}

public class MongoAuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<AuditLog> _collection;

    public MongoAuditLogRepository(IOptions<MongoAuditSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        _collection = db.GetCollection<AuditLog>(settings.Value.CollectionName);
    }

    public Task AddAsync(AuditLog log, CancellationToken cancellationToken = default) =>
        _collection.InsertOneAsync(log, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default) =>
        await _collection.Find(_ => true).SortByDescending(x => x.OccurredAtUtc).Limit(limit).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default) =>
        await _collection.Find(x => x.ResourceType == resourceType && x.ResourceId == resourceId)
            .SortByDescending(x => x.OccurredAtUtc).ToListAsync(cancellationToken);
}
