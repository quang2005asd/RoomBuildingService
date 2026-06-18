namespace RoomBuildingService.Core.Entities;

public class RoomEquipment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public string EquipmentName { get; set; } = null!;
    public short EquipmentIndex { get; set; } = 1;
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Room Room { get; set; } = null!;
}
