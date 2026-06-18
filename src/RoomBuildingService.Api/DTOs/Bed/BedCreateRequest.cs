namespace RoomBuildingService.Api.DTOs.Bed;
using System.ComponentModel.DataAnnotations;
public class BedCreateRequest
{
    [Required] public Guid   RoomId    { get; set; }
    [Required] [MaxLength(20)] public string BedNumber { get; set; } = null!;
}