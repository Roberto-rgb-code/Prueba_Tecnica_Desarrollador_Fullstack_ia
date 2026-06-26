using System.Text;
using System.Text.Json;
using AuditService.Domain.Entities;
using AuditService.Domain.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Toka.Shared.Events;
using Toka.Shared.Messaging;

namespace AuditService.Infrastructure.Messaging;

public class AuditEventConsumer : BackgroundService
{
    private readonly IAuditLogRepository _repository;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<AuditEventConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public AuditEventConsumer(IAuditLogRepository repository, IOptions<RabbitMqSettings> settings, ILogger<AuditEventConsumer> logger)
    {
        _repository = repository;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitForRabbitMqAsync(stoppingToken);

        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync("audit.events", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync("audit.events", _settings.ExchangeName, "#", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                await ProcessEventAsync(ea.RoutingKey, body, stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process audit event");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync("audit.events", autoAck: false, consumer, stoppingToken);
        _logger.LogInformation("Audit consumer started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessEventAsync(string routingKey, string body, CancellationToken cancellationToken)
    {
        var log = routingKey switch
        {
            "user.created" => MapUserCreated(body),
            "user.updated" => MapUserUpdated(body),
            "user.logged_in" => MapUserLoggedIn(body),
            "role.assigned" => MapRoleAssigned(body),
            _ => new AuditLog
            {
                EventType = routingKey,
                ServiceName = "Unknown",
                Action = routingKey,
                Details = body,
                OccurredAtUtc = DateTime.UtcNow
            }
        };

        log.EventId = Guid.NewGuid().ToString();
        await _repository.AddAsync(log, cancellationToken);
        _logger.LogInformation("Audit log stored for {EventType}", log.EventType);
    }

    private static AuditLog MapUserCreated(string body)
    {
        var e = JsonSerializer.Deserialize<UserCreatedEvent>(body)!;
        return new AuditLog
        {
            EventType = "UserCreated",
            ServiceName = "UserService",
            ActorId = e.UserId.ToString(),
            ResourceType = "User",
            ResourceId = e.UserId.ToString(),
            Action = "Create",
            Details = $"User {e.Email} created",
            OccurredAtUtc = e.OccurredAtUtc
        };
    }

    private static AuditLog MapUserUpdated(string body)
    {
        var e = JsonSerializer.Deserialize<UserUpdatedEvent>(body)!;
        return new AuditLog
        {
            EventType = "UserUpdated",
            ServiceName = "UserService",
            ActorId = e.UserId.ToString(),
            ResourceType = "User",
            ResourceId = e.UserId.ToString(),
            Action = "Update",
            Details = $"User {e.Email} updated, active={e.IsActive}",
            OccurredAtUtc = e.OccurredAtUtc
        };
    }

    private static AuditLog MapUserLoggedIn(string body)
    {
        var e = JsonSerializer.Deserialize<UserLoggedInEvent>(body)!;
        return new AuditLog
        {
            EventType = "UserLoggedIn",
            ServiceName = "AuthService",
            ActorId = e.UserId.ToString(),
            ResourceType = "User",
            ResourceId = e.UserId.ToString(),
            Action = "Login",
            Details = $"User {e.Email} logged in",
            OccurredAtUtc = e.OccurredAtUtc
        };
    }

    private static AuditLog MapRoleAssigned(string body)
    {
        var e = JsonSerializer.Deserialize<RoleAssignedEvent>(body)!;
        return new AuditLog
        {
            EventType = "RoleAssigned",
            ServiceName = "RoleService",
            ActorId = e.UserId.ToString(),
            ResourceType = "Role",
            ResourceId = e.RoleId.ToString(),
            Action = "Assign",
            Details = $"Role {e.RoleName} assigned to user {e.UserId}",
            OccurredAtUtc = e.OccurredAtUtc
        };
    }

    private async Task WaitForRabbitMqAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < 30; i++)
        {
            try
            {
                var factory = new ConnectionFactory { HostName = _settings.Host, UserName = _settings.Username, Password = _settings.Password };
                await using var conn = await factory.CreateConnectionAsync(cancellationToken);
                return;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
