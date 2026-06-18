namespace RoomBuildingService.Api.DTOs.Room;
using System.ComponentModel.DataAnnotations;
public class RoomStatusUpdateRequest
{
    [Required] public string  Status            { get; set; } = null!; // AVAILABLE | FULL | UNDER_MAINTENANCE | INACTIVE
    public string? MaintenanceReason { get; set; }          // Bắt buộc điền nếu Status = UNDER_MAINTENANCE
}