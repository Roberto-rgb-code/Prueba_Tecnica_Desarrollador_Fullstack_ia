using AuditService.Domain.Entities;

namespace AuditService.Application.DTOs;

public record AuditLogDto(
    string Id,
    string EventType,
    string ServiceName,
    string ActorId,
    string ResourceType,
    string ResourceId,
    string Action,
    string Details,
    DateTime OccurredAtUtc);

public static class AuditLogMapper
{
    public static AuditLogDto ToDto(AuditLog log) =>
        new(log.Id, log.EventType, log.ServiceName, log.ActorId, log.ResourceType, log.ResourceId, log.Action, log.Details, log.OccurredAtUtc);
}
