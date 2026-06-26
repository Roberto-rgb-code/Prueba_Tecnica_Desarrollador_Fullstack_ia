using Microsoft.Extensions.Logging;

namespace Toka.Shared.Messaging;

public sealed class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;

    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger) => _logger = logger;

    public Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Event {RoutingKey} skipped (RabbitMQ disabled)", routingKey);
        return Task.CompletedTask;
    }
}
