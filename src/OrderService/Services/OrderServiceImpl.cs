using Grpc.Core;
using OrderService.Data;
using OrderService.Infrastructure;
using OrderService.Models;
using OrderService.Protos;

namespace OrderService.Services;

public class OrderServiceImpl : Protos.OrderService.OrderServiceBase
{
    private readonly OrderDbContext _db;
    private readonly IMessagePublisher _publisher;

    public OrderServiceImpl(OrderDbContext db, IMessagePublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            PlacedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = request.Items.Select(i => new Models.OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("order.placed", new
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Items = order.Items.Select(i => new { i.ProductId, i.Quantity })
        });

        return new PlaceOrderResponse { OrderId = order.Id.ToString(), Status = "Pending" };
    }

    public override async Task<OrderStatusResponse> GetOrderStatus(GetOrderStatusRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order ID format"));
        }

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));
        }

        return new OrderStatusResponse
        {
            OrderId = order.Id.ToString(),
            Status = order.Status.ToString()
        };
    }
}