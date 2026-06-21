namespace RoomBuildingService.Api.DTOs.BuildingFacilityPricing;
using System.ComponentModel.DataAnnotations;
public class BuildingFacilityPricingUpdateRequest
{
    [Required] public bool IsPaid { get; set; }
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] public decimal Price { get; set; }
    [Required] public string BillingCycle { get; set; } = null!;
    [Required] public string Status { get; set; } = null!;
    public string? Description { get; set; }
}
