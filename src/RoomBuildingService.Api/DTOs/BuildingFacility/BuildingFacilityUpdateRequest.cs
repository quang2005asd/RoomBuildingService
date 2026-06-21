namespace RoomBuildingService.Api.DTOs.BuildingFacility;
using System.ComponentModel.DataAnnotations;
public class BuildingFacilityUpdateRequest
{
    [Required] [MaxLength(100)] public string FacilityName { get; set; } = null!;
    [Required] public string Status { get; set; } = null!;
    public string? Description { get; set; }
}
