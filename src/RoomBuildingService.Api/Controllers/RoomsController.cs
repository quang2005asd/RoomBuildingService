namespace RoomBuildingService.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Api.DTOs.Bed;
using RoomBuildingService.Api.DTOs.Building;
using RoomBuildingService.Api.DTOs.Equipment;
using RoomBuildingService.Api.DTOs.Room;
using RoomBuildingService.Api.DTOs.RoomType;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class RoomsController(IRoomRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? buildingId,
        [FromQuery] int? floor,
        [FromQuery] string? status)
    {
        var rooms = await repo.GetAllAsync(buildingId, floor, status);
        return Ok(rooms.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var room = await repo.GetByIdAsync(id);
        if (room is null) return NotFound();

        return Ok(MapToResponse(room));
    }

    [HttpGet("floormap")]
    public async Task<IActionResult> GetFloorMap([FromQuery] Guid buildingId, [FromQuery] int floor)
    {
        var rooms = await repo.GetByFloorAsync(buildingId, floor);
        return Ok(rooms.Select(MapToResponse));
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] Guid? buildingId,
        [FromQuery] Guid? roomTypeId,
        [FromQuery] string? genderType,
        [FromQuery] DateOnly? expectedStartDate,
        [FromQuery] DateOnly? expectedEndDate)
    {
        if (expectedStartDate.HasValue && expectedEndDate.HasValue && expectedStartDate > expectedEndDate)
            throw new BusinessRuleException("ExpectedStartDate must be earlier than or equal to ExpectedEndDate.");

        var rooms = await repo.GetAvailableAsync(buildingId, roomTypeId, genderType, expectedStartDate, expectedEndDate);
        var result = rooms.Select(room => new AvailableRoomResponse
        {
            Id = room.Id,
            BuildingId = room.BuildingId,
            RoomTypeId = room.RoomTypeId,
            RoomNumber = room.RoomNumber,
            FloorNumber = room.FloorNumber,
            Status = room.Status,
            CurrentOccupancy = GetCurrentOccupancy(room),
            AvailableSlots = GetAvailableSlots(room),
            Building = MapBuilding(room.Building),
            RoomType = MapRoomType(room.RoomType),
            ExpectedStartDate = expectedStartDate,
            ExpectedEndDate = expectedEndDate,
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoomCreateRequest req)
    {
        var room = new Room
        {
            BuildingId = req.BuildingId,
            RoomTypeId = req.RoomTypeId,
            RoomNumber = req.RoomNumber,
            FloorNumber = req.FloorNumber,
            ImageUrl = req.ImageUrl,
        };

        var created = await repo.CreateAsync(room);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoomUpdateRequest req)
    {
        var room = new Room
        {
            Id = id,
            RoomTypeId = req.RoomTypeId,
            RoomNumber = req.RoomNumber,
            FloorNumber = req.FloorNumber,
            ImageUrl = req.ImageUrl,
        };

        var updated = await repo.UpdateAsync(room);
        return Ok(MapToResponse(updated));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] RoomStatusUpdateRequest req)
    {
        if (req.Status == "UNDER_MAINTENANCE" && string.IsNullOrWhiteSpace(req.MaintenanceReason))
            throw new BusinessRuleException("MaintenanceReason is required when room status is UNDER_MAINTENANCE.");

        await repo.UpdateStatusAsync(id, req.Status, req.MaintenanceReason);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await repo.DeleteAsync(id);
        return NoContent();
    }

    private static RoomResponse MapToResponse(Room room) => new()
    {
        Id = room.Id,
        BuildingId = room.BuildingId,
        RoomTypeId = room.RoomTypeId,
        RoomNumber = room.RoomNumber,
        FloorNumber = room.FloorNumber,
        ImageUrl = room.ImageUrl,
        CurrentOccupancy = GetCurrentOccupancy(room),
        AvailableSlots = GetAvailableSlots(room),
        Status = room.Status,
        MaintenanceReason = room.MaintenanceReason,
        Building = MapBuilding(room.Building),
        RoomType = MapRoomType(room.RoomType),
        Beds = (room.Beds ?? []).Select(MapBed).ToList(),
        Equipments = (room.Equipments ?? []).Select(MapEquipment).ToList(),
        CreatedAt = room.CreatedAt,
        UpdatedAt = room.UpdatedAt,
    };

    private static int GetCurrentOccupancy(Room room) => room.Beds?.Count(b => b.Status == "OCCUPIED") ?? room.CurrentOccupancy;

    private static int GetAvailableSlots(Room room)
    {
        var capacity = room.RoomType?.Capacity ?? 0;
        var occupancy = GetCurrentOccupancy(room);
        return Math.Max(0, capacity - occupancy);
    }

    private static BuildingResponse? MapBuilding(Building? building)
    {
        if (building is null) return null;

        return new BuildingResponse
        {
            Id = building.Id,
            Name = building.Name,
            TotalFloors = building.TotalFloors,
            Floors = Enumerable.Range(1, building.TotalFloors).ToList(),
            GenderType = building.GenderType,
            Status = building.Status,
            Description = building.Description,
            CreatedAt = building.CreatedAt,
            UpdatedAt = building.UpdatedAt,
        };
    }

    private static RoomTypeResponse? MapRoomType(RoomType? roomType)
    {
        if (roomType is null) return null;

        return new RoomTypeResponse
        {
            Id = roomType.Id,
            TypeName = roomType.TypeName,
            Capacity = roomType.Capacity,
            BasePrice = roomType.BasePrice,
            Description = roomType.Description,
            Amenities = roomType.Amenities?.Select(a => a.AmenityName).ToList() ?? [],
            CreatedAt = roomType.CreatedAt,
            UpdatedAt = roomType.UpdatedAt,
        };
    }

    private static BedResponse MapBed(Bed bed) => new()
    {
        Id = bed.Id,
        RoomId = bed.RoomId,
        BedNumber = bed.BedNumber,
        Status = bed.Status,
        StudentId = bed.StudentId,
        StudentName = bed.StudentName,
        StudentCode = bed.StudentCode,
        CreatedAt = bed.CreatedAt,
        UpdatedAt = bed.UpdatedAt,
    };

    private static EquipmentResponse MapEquipment(RoomEquipment equipment) => new()
    {
        Id = equipment.Id,
        RoomId = equipment.RoomId,
        EquipmentName = equipment.EquipmentName,
        EquipmentIndex = equipment.EquipmentIndex,
        Status = equipment.Status,
        CreatedAt = equipment.CreatedAt,
        UpdatedAt = equipment.UpdatedAt,
    };
}
