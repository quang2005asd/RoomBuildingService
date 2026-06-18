namespace RoomBuildingService.Core.Entities;

public class RoomAmenity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomTypeId { get; set; }
    public string AmenityName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public RoomType RoomType { get; set; } = null!;
}
