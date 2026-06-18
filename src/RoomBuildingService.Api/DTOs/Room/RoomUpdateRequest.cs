namespace RoomBuildingService.Api.DTOs.Room;
using System.ComponentModel.DataAnnotations;
public class RoomUpdateRequest
{
    [Required] public Guid   RoomTypeId  { get; set; }
    [Required] [MaxLength(20)] public string RoomNumber  { get; set; } = null!;
    [Required] [Range(1, 100)] public int    FloorNumber { get; set; }
    public string? ImageUrl { get; set; }
}
