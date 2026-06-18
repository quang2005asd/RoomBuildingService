namespace RoomBuildingService.Infrastructure.BackgroundWorkers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoomBuildingService.Infrastructure.Messaging;
using RoomBuildingService.Infrastructure.Persistence;
public class OutboxWorker(
IServiceScopeFactory scopeFactory,
RabbitMQPublisher    publisher,
ILogger<OutboxWorker> logger) : BackgroundService
{
private const int BatchSize  = 10;   // Xử lý 10 event mỗi lần
private const int MaxRetry   = 5;    // Thử tối đa 5 lần rồi đánh dấu FAILED
private const int DelayMs    = 5000; // Quét mỗi 5 giây
protected override async Task ExecuteAsync(CancellationToken ct)
{
    logger.LogInformation("OutboxWorker started.");

    while (!ct.IsCancellationRequested)
    {
        try
        {
            await ProcessPendingEventsAsync(ct);
        }
        catch (Exception ex)
        {
            // Không crash worker khi gặp lỗi bất ngờ
            logger.LogError(ex, "OutboxWorker encountered an error.");
        }

        await Task.Delay(DelayMs, ct);
    }
}

private async Task ProcessPendingEventsAsync(CancellationToken ct)
{
    // Dùng scope riêng vì DbContext là Scoped, Worker là Singleton
    using var scope = scopeFactory.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var events = await db.OutboxEvents
        .Where(e => e.Status == "PENDING" && e.RetryCount < MaxRetry)
        .OrderBy(e => e.CreatedAt)
        .Take(BatchSize)
        .ToListAsync(ct);

    if (!events.Any()) return;

    logger.LogInformation("Processing {Count} outbox events.", events.Count);

    foreach (var evt in events)
    {
        try
        {
            await publisher.PublishAsync(evt.EventType, evt.Payload);

            evt.Status    = "SENT";
            evt.SentAt    = DateTime.UtcNow;
            logger.LogInformation("Event sent: {EventType} | Id: {Id}", evt.EventType, evt.Id);
        }
        catch (Exception ex)
        {
            evt.RetryCount++;

            if (evt.RetryCount >= MaxRetry)
            {
                evt.Status = "FAILED";
                logger.LogError(ex,
                    "Event FAILED after {MaxRetry} retries | Id: {Id}", MaxRetry, evt.Id);
            }
            else
            {
                logger.LogWarning(ex,
                    "Event retry {Retry}/{Max} | Id: {Id}", evt.RetryCount, MaxRetry, evt.Id);
            }
        }
    }

    await db.SaveChangesAsync(ct);
}
}