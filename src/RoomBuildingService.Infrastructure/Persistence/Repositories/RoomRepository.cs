namespace RoomBuildingService.Infrastructure.Persistence.Repositories;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;

public class RoomRepository(AppDbContext db) : IRoomRepository
{
    public async Task<IEnumerable<Room>> GetAllAsync(Guid? buildingId, int? floor, string? status)
    {
        var query = db.Rooms.AsNoTracking()
            .Include(r => r.Building)
            .Include(r => r.RoomType).ThenInclude(rt => rt.Amenities)
            .Include(r => r.Beds)
            .Include(r => r.Equipments)
            .AsQueryable();

        if (buildingId.HasValue) query = query.Where(r => r.BuildingId == buildingId);
        if (floor.HasValue) query = query.Where(r => r.FloorNumber == floor);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(r => r.Status == status);

        return await query.OrderBy(r => r.BuildingId).ThenBy(r => r.FloorNumber).ThenBy(r => r.RoomNumber).ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetByFloorAsync(Guid buildingId, int floor)
        => await db.Rooms.AsNoTracking()
            .Include(r => r.Building)
            .Include(r => r.RoomType).ThenInclude(rt => rt.Amenities)
            .Include(r => r.Beds)
            .Include(r => r.Equipments)
            .Where(r => r.BuildingId == buildingId && r.FloorNumber == floor)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

    public async Task<IEnumerable<Room>> GetAvailableAsync(Guid? buildingId, Guid? roomTypeId, string? genderType, DateOnly? expectedStartDate, DateOnly? expectedEndDate)
    {
        _ = expectedStartDate;
        _ = expectedEndDate;

        var normalizedGenderType = NormalizeGenderType(genderType);

        var query = db.Rooms.AsNoTracking()
            .Include(r => r.Building)
            .Include(r => r.RoomType).ThenInclude(rt => rt.Amenities)
            .Include(r => r.Beds)
            .Include(r => r.Equipments)
            .Where(r => r.Status == "AVAILABLE");

        if (buildingId.HasValue) query = query.Where(r => r.BuildingId == buildingId);
        if (roomTypeId.HasValue) query = query.Where(r => r.RoomTypeId == roomTypeId);
        if (!string.IsNullOrWhiteSpace(normalizedGenderType))
            query = query.Where(r => r.Building.GenderType == normalizedGenderType || r.Building.GenderType == "MIXED");

        var rooms = await query
            .OrderBy(r => r.BuildingId)
            .ThenBy(r => r.FloorNumber)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();

        return rooms.Where(r => CalculateAvailableSlots(r) > 0);
    }

    public async Task<Room?> GetByIdAsync(Guid id)
        => await db.Rooms.AsNoTracking()
            .Include(r => r.Building)
            .Include(r => r.RoomType).ThenInclude(rt => rt.Amenities)
            .Include(r => r.Beds)
            .Include(r => r.Equipments)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Room> CreateAsync(Room room)
    {
        var building = await db.Buildings.FindAsync(room.BuildingId)
            ?? throw new NotFoundException("Building", room.BuildingId);

        if (room.FloorNumber > building.TotalFloors)
            throw new BusinessRuleException($"FloorNumber ({room.FloorNumber}) exceeds TotalFloors ({building.TotalFloors}).");

        var roomType = await db.RoomTypes.FindAsync(room.RoomTypeId)
            ?? throw new NotFoundException("RoomType", room.RoomTypeId);

        room.Id = Guid.NewGuid();
        room.CreatedAt = DateTime.UtcNow;
        room.Status = "AVAILABLE";
        room.CurrentOccupancy = 0;
        room.ImageUrl = NormalizeImageUrl(room.ImageUrl);

        for (var i = 1; i <= roomType.Capacity; i++)
        {
            room.Beds.Add(new Bed
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                BedNumber = $"{room.RoomNumber}-{i:D2}",
                Status = "AVAILABLE",
                CreatedAt = DateTime.UtcNow,
            });
        }

        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        return await GetTrackedRoomAsync(room.Id);
    }

    public async Task<Room> UpdateAsync(Room room)
    {
        var existing = await db.Rooms.FindAsync(room.Id)
            ?? throw new NotFoundException("Room", room.Id);

        if (room.FloorNumber != existing.FloorNumber)
        {
            var building = await db.Buildings.FindAsync(existing.BuildingId);
            if (building is not null && room.FloorNumber > building.TotalFloors)
                throw new BusinessRuleException($"FloorNumber ({room.FloorNumber}) exceeds TotalFloors ({building.TotalFloors}).");
        }

        if (!await db.RoomTypes.AnyAsync(rt => rt.Id == room.RoomTypeId))
            throw new NotFoundException("RoomType", room.RoomTypeId);

        existing.RoomNumber = room.RoomNumber;
        existing.RoomTypeId = room.RoomTypeId;
        existing.FloorNumber = room.FloorNumber;
        existing.ImageUrl = NormalizeImageUrl(room.ImageUrl);
        existing.UpdatedAt = DateTime.UtcNow;

        await SyncRoomStatusAsync(existing, null);
        await db.SaveChangesAsync();

        return await GetTrackedRoomAsync(existing.Id);
    }

    public async Task UpdateStatusAsync(Guid id, string status, string? maintenanceReason)
    {
        var existing = await db.Rooms
            .Include(r => r.Building)
            .Include(r => r.RoomType).ThenInclude(rt => rt.Amenities)
            .Include(r => r.Beds)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Room", id);

        var normalizedStatus = NormalizeRoomStatus(status);
        if (normalizedStatus == "FULL" && CalculateAvailableSlots(existing) > 0)
            throw new BusinessRuleException("Room still has available beds, so it cannot be forced to FULL.");

        if (normalizedStatus is "UNDER_MAINTENANCE" or "INACTIVE" && CalculateOccupancy(existing) > 0)
            throw new BusinessRuleException("Cannot move a room to UNDER_MAINTENANCE or INACTIVE while it still has occupied beds.");

        await SyncRoomStatusAsync(existing, normalizedStatus, maintenanceReason);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await db.Rooms
            .Include(r => r.Beds)
            .Include(r => r.Equipments)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("Room", id);

        existing.CurrentOccupancy = CalculateOccupancy(existing);
        if (existing.CurrentOccupancy > 0)
            throw new BusinessRuleException("Cannot delete a room that still has occupied beds.");

        if (existing.Equipments.Any(e => e.Status == "ACTIVE" || e.Status == "UNDER_MAINTENANCE" || e.Status == "BROKEN"))
            throw new BusinessRuleException("Cannot delete a room that still has equipment records. Remove equipment first.");

        db.Rooms.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await db.Rooms.AnyAsync(r => r.Id == id);

    private async Task<Room> GetTrackedRoomAsync(Guid id)
        => await db.Rooms.AsNoTracking()
            .Include(r => r.Building)
            .Include(r => r.RoomType).ThenInclude(rt => rt.Amenities)
            .Include(r => r.Beds)
            .Include(r => r.Equipments)
            .FirstAsync(r => r.Id == id);

    private async Task SyncRoomStatusAsync(Room room, string? requestedStatus, string? maintenanceReason = null)
    {
        var previousStatus = room.Status;
        var occupancy = CalculateOccupancy(room);
        var availableSlots = CalculateAvailableSlots(room);

        room.CurrentOccupancy = occupancy;
        room.MaintenanceReason = requestedStatus == "UNDER_MAINTENANCE" ? maintenanceReason : null;

        if (requestedStatus == "UNDER_MAINTENANCE")
        {
            room.Status = "UNDER_MAINTENANCE";
        }
        else if (requestedStatus == "INACTIVE")
        {
            room.Status = "INACTIVE";
        }
        else
        {
            room.Status = availableSlots <= 0 ? "FULL" : "AVAILABLE";
        }

        room.UpdatedAt = DateTime.UtcNow;

        if (!string.Equals(previousStatus, room.Status, StringComparison.Ordinal))
            await AddRoomStatusChangedEventAsync(room, previousStatus);
    }

    private async Task AddRoomStatusChangedEventAsync(Room room, string previousStatus)
    {
        var payload = JsonSerializer.Serialize(new
        {
            roomId = room.Id,
            buildingId = room.BuildingId,
            roomTypeId = room.RoomTypeId,
            roomNumber = room.RoomNumber,
            previousStatus,
            currentStatus = room.Status,
            currentOccupancy = room.CurrentOccupancy,
            maintenanceReason = room.MaintenanceReason,
            changedAt = DateTime.UtcNow,
        });

        await db.OutboxEvents.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "room.status.changed",
            AggregateId = room.Id,
            Payload = payload,
            Status = "PENDING",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
        });
    }

    private static int CalculateOccupancy(Room room) => room.Beds.Count(b => b.Status == "OCCUPIED");

    private static int CalculateAvailableSlots(Room room)
    {
        var capacity = room.RoomType?.Capacity ?? room.Beds.Count;
        return Math.Max(0, capacity - CalculateOccupancy(room));
    }

    private static string NormalizeGenderType(string? genderType)
    {
        if (string.IsNullOrWhiteSpace(genderType)) return string.Empty;

        var key = genderType.Trim().ToUpperInvariant();
        return key switch
        {
            "MALE" or "NAM" => "MALE",
            "FEMALE" or "NU" => "FEMALE",
            "MIXED" or "HONHOP" or "HON_HOP" => "MIXED",
            _ => throw new BusinessRuleException("GenderType must be MALE/FEMALE/MIXED (or Nam/Nu/HonHop)."),
        };
    }

    private static string NormalizeRoomStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new BusinessRuleException("Status is required.");

        var key = status.Trim().ToUpperInvariant();
        return key switch
        {
            "AVAILABLE" => "AVAILABLE",
            "FULL" => "FULL",
            "UNDER_MAINTENANCE" => "UNDER_MAINTENANCE",
            "INACTIVE" => "INACTIVE",
            _ => throw new BusinessRuleException("Status must be AVAILABLE/FULL/UNDER_MAINTENANCE/INACTIVE."),
        };
    }

    private static string? NormalizeImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        return imageUrl.Trim();
    }
}
