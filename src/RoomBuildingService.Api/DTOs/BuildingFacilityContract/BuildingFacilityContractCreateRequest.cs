namespace RoomBuildingService.Api.DTOs.BuildingFacilityContract;
using System.ComponentModel.DataAnnotations;
public class BuildingFacilityContractCreateRequest
{
    [Required] public Guid FacilityId { get; set; }
    [Required] public Guid PricingId { get; set; }
    [Required] [MaxLength(50)] public string ContractCode { get; set; } = null!;
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
    [Required] public string ContractType { get; set; } = null!;
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
}
