namespace OrderService.Models;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public DateTime PlacedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}

public enum OrderStatus { Pending, Confirmed, Shipped, Delivered }