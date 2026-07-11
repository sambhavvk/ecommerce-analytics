namespace OrderService.Infrastructure;

public interface IMessagePublisher
{
    Task PublishAsync(string routingKey, object message);
}