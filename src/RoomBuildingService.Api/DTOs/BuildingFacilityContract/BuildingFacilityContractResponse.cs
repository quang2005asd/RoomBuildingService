namespace RoomBuildingService.Api.DTOs.BuildingFacilityContract;
public class BuildingFacilityContractResponse
{
    public Guid Id { get; set; }
    public Guid FacilityId { get; set; }
    public Guid PricingId { get; set; }
    public string ContractCode { get; set; } = null!;
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
    public string ContractType { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
