using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Toka.Shared.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default);
}

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqEventPublisher(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqEventPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        Exception? lastError = null;
        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Topic, durable: true).GetAwaiter().GetResult();
                _logger.LogInformation("Connected to RabbitMQ at {Host}", _settings.Host);
                return;
            }
            catch (Exception ex)
            {
                lastError = ex;
                _logger.LogWarning("RabbitMQ connection attempt {Attempt} failed, retrying...", attempt + 1);
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        throw new InvalidOperationException("Could not connect to RabbitMQ after multiple attempts.", lastError);
    }

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties { ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent };

        await _channel.BasicPublishAsync(_settings.ExchangeName, routingKey, false, props, body, cancellationToken);
        _logger.LogInformation("Published event {RoutingKey}", routingKey);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
