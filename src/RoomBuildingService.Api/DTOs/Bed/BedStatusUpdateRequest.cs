namespace RoomBuildingService.Api.DTOs.Bed;
using System.ComponentModel.DataAnnotations;
public class BedStatusUpdateRequest
{
    [Required] public string Status { get; set; } = null!;
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
}
