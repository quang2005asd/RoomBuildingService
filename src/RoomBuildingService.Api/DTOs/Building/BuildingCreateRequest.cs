namespace RoomBuildingService.Api.DTOs.Building;
using System.ComponentModel.DataAnnotations;
public class BuildingCreateRequest
{
    [Required] [MaxLength(50)] public string Name        { get; set; } = null!;
    [Required] [Range(1, 100)] public int    TotalFloors { get; set; }
    [Required]                 public string GenderType  { get; set; } = null!; // MALE | FEMALE | MIXED (Nam | Nu | HonHop)
    public string? Description { get; set; }
}