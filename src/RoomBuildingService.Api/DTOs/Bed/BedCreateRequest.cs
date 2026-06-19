namespace RoomBuildingService.Api.DTOs.Bed;
using System.ComponentModel.DataAnnotations;
public class BedCreateRequest
{
    [Required] public Guid   RoomId    { get; set; }
    [Required] [MaxLength(20)] public string BedNumber { get; set; } = null!;
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
}
