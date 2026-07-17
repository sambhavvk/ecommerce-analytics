using System.Text;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using AnalyticsService.Hubs;

namespace AnalyticsService.Services;

public class EventConsumerService : BackgroundService
{
    private readonly IHubContext<AnalyticsHub> _hubContext;
    private readonly IDatabase _redis;
    private IConnection? _connection;
    private IChannel? _channel;

    public EventConsumerService(IHubContext<AnalyticsHub> hubContext, IConnectionMultiplexer redis)
    {
        _hubContext = hubContext;
        _redis = redis.GetDatabase();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost", AutomaticRecoveryEnabled = true };
                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                await _channel.QueueDeclareAsync(queue: "analytics_queue", durable: false, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    await _redis.StringIncrementAsync("page_views");
                    await _hubContext.Clients.All.SendAsync("PageViewUpdate", message, stoppingToken);
                };

                await _channel.BasicConsumeAsync(queue: "analytics_queue", autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

                // Keep running until cancellation
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                // RabbitMQ not available, retry after delay
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
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