namespace RoomBuildingService.Api.DTOs.Equipment;
public class EquipmentResponse
{
    public Guid      Id             { get; set; }
    public Guid      RoomId         { get; set; }
    public string    EquipmentName  { get; set; } = null!;
    public short     EquipmentIndex { get; set; }
    public string    Status         { get; set; } = null!;
    public DateTime  CreatedAt      { get; set; }
    public DateTime? UpdatedAt      { get; set; }
}