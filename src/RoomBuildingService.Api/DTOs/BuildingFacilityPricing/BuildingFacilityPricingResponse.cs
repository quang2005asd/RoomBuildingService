namespace RoomBuildingService.Api.DTOs.BuildingFacilityPricing;
public class BuildingFacilityPricingResponse
{
    public Guid Id { get; set; }
    public Guid FacilityId { get; set; }
    public bool IsPaid { get; set; }
    public decimal Price { get; set; }
    public string BillingCycle { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
