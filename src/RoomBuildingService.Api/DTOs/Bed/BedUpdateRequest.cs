namespace RoomBuildingService.Api.DTOs.Bed;
using System.ComponentModel.DataAnnotations;
public class BedUpdateRequest
{
    [Required] [MaxLength(20)] public string BedNumber { get; set; } = null!;
    [Required]                 public string Status    { get; set; } = null!;
}