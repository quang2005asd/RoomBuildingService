namespace RoomBuildingService.Core.Entities;

public class OutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public Guid AggregateId { get; set; }
    public string Payload { get; set; } = null!;
    public string Status { get; set; } = "PENDING";
    public short RetryCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
}
