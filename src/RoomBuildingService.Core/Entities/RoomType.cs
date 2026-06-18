namespace RoomBuildingService.Core.Entities;

public class RoomType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TypeName { get; set; } = null!;
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<RoomAmenity> Amenities { get; set; } = new List<RoomAmenity>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
