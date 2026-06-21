namespace RoomBuildingService.Core.Entities;

public class BuildingFacility
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BuildingId { get; set; }
    public string FacilityName { get; set; } = null!;
    public string Status { get; set; } = "ACTIVE";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Building Building { get; set; } = null!;
    public ICollection<BuildingFacilityPricing> Pricings { get; set; } = new List<BuildingFacilityPricing>();
    public ICollection<BuildingFacilityContract> Contracts { get; set; } = new List<BuildingFacilityContract>();
}
