using StackExchange.Redis;
using AnalyticsService.Hubs;
using AnalyticsService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddHostedService<EventConsumerService>();

var app = builder.Build();
app.MapHub<AnalyticsHub>("/analytics-hub");
app.Run();