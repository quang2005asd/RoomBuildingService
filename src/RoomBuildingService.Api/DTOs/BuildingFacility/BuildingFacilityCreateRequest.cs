namespace RoomBuildingService.Api.DTOs.BuildingFacility;
using System.ComponentModel.DataAnnotations;
public class BuildingFacilityCreateRequest
{
    [Required] public Guid BuildingId { get; set; }
    [Required] [MaxLength(100)] public string FacilityName { get; set; } = null!;
    public string? Description { get; set; }
}
