using System.Text;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using AnalyticsService.Hubs;

namespace AnalyticsService.Services;

public class EventConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly IHubContext<AnalyticsHub> _hubContext;
    private readonly IDatabase _redis;

    public EventConsumerService(IHubContext<AnalyticsHub> hubContext, IConnectionMultiplexer redis)
    {
        _hubContext = hubContext;
        _redis = redis.GetDatabase();

        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _channel.QueueDeclareAsync(queue: "analytics_queue", durable: false, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await _redis.StringIncrementAsync("page_views");
            await _hubContext.Clients.All.SendAsync("PageViewUpdate", message, stoppingToken);
        };

        _channel.BasicConsumeAsync(queue: "analytics_queue", autoAck: true, consumer: consumer).GetAwaiter().GetResult();

        return Task.CompletedTask;
    }

    public override async void Dispose()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
        base.Dispose();
    }
}