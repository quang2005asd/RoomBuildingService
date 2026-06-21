namespace RoomBuildingService.Core.Entities;

public class BuildingFacilityContract
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FacilityId { get; set; }
    public Guid PricingId { get; set; }
    public string ContractCode { get; set; } = null!;
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
    public string ContractType { get; set; } = "MONTHLY";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public BuildingFacility Facility { get; set; } = null!;
    public BuildingFacilityPricing Pricing { get; set; } = null!;
}
