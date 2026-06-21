namespace RoomBuildingService.Core.Entities;

public class BuildingFacilityPricing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FacilityId { get; set; }
    public bool IsPaid { get; set; }
    public decimal Price { get; set; }
    public string BillingCycle { get; set; } = "FREE";
    public string Status { get; set; } = "ACTIVE";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public BuildingFacility Facility { get; set; } = null!;
    public ICollection<BuildingFacilityContract> Contracts { get; set; } = new List<BuildingFacilityContract>();
}
