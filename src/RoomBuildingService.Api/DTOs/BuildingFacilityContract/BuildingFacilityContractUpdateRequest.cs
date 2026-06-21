namespace RoomBuildingService.Api.DTOs.BuildingFacilityContract;
using System.ComponentModel.DataAnnotations;
public class BuildingFacilityContractUpdateRequest
{
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
    [Required] public string ContractType { get; set; } = null!;
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] public decimal TotalAmount { get; set; }
    [Required] public string Status { get; set; } = null!;
    public string? Notes { get; set; }
}
