namespace RoomBuildingService.Core.Entities;

public class Bed
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public string BedNumber { get; set; } = null!;
    public string Status { get; set; } = "AVAILABLE";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Room Room { get; set; } = null!;
}
