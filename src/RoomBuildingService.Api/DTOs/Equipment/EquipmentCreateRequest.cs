namespace RoomBuildingService.Api.DTOs.Equipment;
using System.ComponentModel.DataAnnotations;
public class EquipmentCreateRequest
{
    [Required] public Guid   RoomId        { get; set; }
    [Required] [MaxLength(100)] public string EquipmentName { get; set; } = null!; // 'Máy lạnh', 'Bình nóng lạnh'
}