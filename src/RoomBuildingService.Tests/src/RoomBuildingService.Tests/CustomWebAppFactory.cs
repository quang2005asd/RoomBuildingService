namespace RoomBuildingService.Tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoomBuildingService.Infrastructure.Persistence;
public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
{
["UseInMemoryDatabase"] = "true"
});
});

builder.ConfigureServices(services =>
{
// Xóa DbContext thật, thay bằng InMemory
var descriptor = services.SingleOrDefault(
d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
if (descriptor != null)
services.Remove(descriptor);
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseInMemoryDatabase("TestDb_" + Guid.NewGuid())); // Mỗi test dùng DB riêng

        // Tắt OutboxWorker khi test (không cần RabbitMQ)
        var outbox = services.SingleOrDefault(
            d => d.ImplementationType?.Name == "OutboxWorker");
        if (outbox != null)
            services.Remove(outbox);
    });
}
}
