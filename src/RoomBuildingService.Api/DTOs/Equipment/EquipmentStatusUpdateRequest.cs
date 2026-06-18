namespace RoomBuildingService.Api.DTOs.Equipment;
using System.ComponentModel.DataAnnotations;
public class EquipmentStatusUpdateRequest
{
    [Required] public string Status { get; set; } = null!; // ACTIVE | UNDER_MAINTENANCE | BROKEN | RETIRED
}