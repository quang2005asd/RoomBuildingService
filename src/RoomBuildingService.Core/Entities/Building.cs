namespace RoomBuildingService.Core.Entities;

public class Building
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public int TotalFloors { get; set; }
    public string GenderType { get; set; } = null!;
    public string Status { get; set; } = "ACTIVE";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<BuildingFacility> Facilities { get; set; } = new List<BuildingFacility>();
}
