namespace Toka.Shared.Events;

public record AuditEventMessage(
    string EventId,
    string EventType,
    string ServiceName,
    string ActorId,
    string ResourceType,
    string ResourceId,
    string Action,
    string Details,
    DateTime OccurredAtUtc);

public record UserCreatedEvent(
    Guid UserId,
    string Email,
    string FullName,
    DateTime OccurredAtUtc);

public record UserUpdatedEvent(
    Guid UserId,
    string Email,
    string FullName,
    bool IsActive,
    DateTime OccurredAtUtc);

public record RoleAssignedEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    DateTime OccurredAtUtc);

public record UserLoggedInEvent(
    Guid UserId,
    string Email,
    DateTime OccurredAtUtc);
