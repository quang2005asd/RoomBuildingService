namespace RoomBuildingService.Api.DTOs.Room;
using RoomBuildingService.Api.DTOs.Building;
using RoomBuildingService.Api.DTOs.RoomType;

public class AvailableRoomResponse
{
    public Guid                  Id               { get; set; }
    public Guid                  BuildingId       { get; set; }
    public Guid                  RoomTypeId       { get; set; }
    public string                RoomNumber       { get; set; } = null!;
    public int                   FloorNumber      { get; set; }
    public string                Status           { get; set; } = null!;
    public int                   CurrentOccupancy { get; set; }
    public int                   AvailableSlots   { get; set; }
    public BuildingResponse?     Building         { get; set; }
    public RoomTypeResponse?     RoomType         { get; set; }
    public DateOnly?             ExpectedStartDate { get; set; }
    public DateOnly?             ExpectedEndDate   { get; set; }
}
