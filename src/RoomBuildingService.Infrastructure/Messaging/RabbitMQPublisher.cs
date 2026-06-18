namespace RoomBuildingService.Infrastructure.Messaging;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
public class RabbitMQPublisher : IAsyncDisposable
{
private readonly ILogger<RabbitMQPublisher> _logger;
private IConnection? _connection;
private IChannel?    _channel;
private readonly string _host;
private readonly int    _port;
private readonly string _username;
private readonly string _password;

public RabbitMQPublisher(IConfiguration config, ILogger<RabbitMQPublisher> logger)
{
    _logger   = logger;
    _host     = config["RabbitMQ:Host"]     ?? "localhost";
    _port     = int.Parse(config["RabbitMQ:Port"] ?? "5672");
    _username = config["RabbitMQ:Username"] ?? "guest";
    _password = config["RabbitMQ:Password"] ?? "guest";
}

// Kết nối lazy — chỉ kết nối khi lần đầu publish
private async Task EnsureConnectedAsync()
{
    if (_connection?.IsOpen == true && _channel?.IsOpen == true)
        return;

    var factory = new ConnectionFactory
    {
        HostName = _host,
        Port     = _port,
        UserName = _username,
        Password = _password
    };

    _connection = await factory.CreateConnectionAsync();
    _channel    = await _connection.CreateChannelAsync();

    // Khai báo exchange kiểu topic — các service khác subscribe theo routing key
    await _channel.ExchangeDeclareAsync(
        exchange:    "ktx.events",
        type:        ExchangeType.Topic,
        durable:     true,   // Không mất khi RabbitMQ restart
        autoDelete:  false
    );

    _logger.LogInformation("RabbitMQ connected: {Host}:{Port}", _host, _port);
}

public async Task PublishAsync(string eventType, string payload)
{
    await EnsureConnectedAsync();

    var body = Encoding.UTF8.GetBytes(payload);

    var props = new BasicProperties
    {
        ContentType  = "application/json",
        DeliveryMode = DeliveryModes.Persistent,  // Không mất message khi RabbitMQ restart
        Timestamp    = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
    };

    // Publish lên exchange với routing key = eventType (vd: room.status.changed)
    await _channel!.BasicPublishAsync(
        exchange:   "ktx.events",
        routingKey: eventType,
        mandatory:  false,
        basicProperties: props,
        body:       body
    );

    _logger.LogInformation("Published event: {EventType}", eventType);
}

public async ValueTask DisposeAsync()
{
    if (_channel is not null) await _channel.DisposeAsync();
    if (_connection is not null) await _connection.DisposeAsync();
}
}