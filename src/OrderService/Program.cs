using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Infrastructure;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));

builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<OrderServiceImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();