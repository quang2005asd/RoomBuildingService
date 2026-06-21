namespace RoomBuildingService.Infrastructure.Persistence.Repositories;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;

public class BedRepository(AppDbContext db) : IBedRepository
{
    public async Task<IEnumerable<Bed>> GetByRoomAsync(Guid roomId)
        => await db.Beds.AsNoTracking()
            .Where(b => b.RoomId == roomId)
            .OrderBy(b => b.BedNumber)
            .ToListAsync();

    public async Task<Bed?> GetByIdAsync(Guid id)
        => await db.Beds.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Bed> CreateAsync(Bed bed)
    {
        var room = await db.Rooms
            .Include(r => r.RoomType)
            .Include(r => r.Beds)
            .FirstOrDefaultAsync(r => r.Id == bed.RoomId)
            ?? throw new NotFoundException("Room", bed.RoomId);

        if (await db.Beds.AnyAsync(b => b.RoomId == bed.RoomId && b.BedNumber == bed.BedNumber))
            throw new BusinessRuleException($"Bed '{bed.BedNumber}' already exists in this room.");

        bed.Id = Guid.NewGuid();
        bed.CreatedAt = DateTime.UtcNow;
        bed.Status = NormalizeBedStatus(bed.Status, fallback: "AVAILABLE");
        ApplyStudentOccupancyFields(bed);

        db.Beds.Add(bed);
        room.Beds.Add(bed);

        await SyncRoomByBedsAsync(room);
        await db.SaveChangesAsync();
        return bed;
    }

    public async Task<Bed> UpdateAsync(Bed bed)
    {
        var existing = await db.Beds
            .Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.Room).ThenInclude(r => r.Beds)
            .FirstOrDefaultAsync(b => b.Id == bed.Id)
            ?? throw new NotFoundException("Bed", bed.Id);

        var normalizedStatus = NormalizeBedStatus(bed.Status, fallback: existing.Status);
        if (existing.Status == "OCCUPIED" && normalizedStatus is "UNDER_MAINTENANCE" or "INACTIVE")
            throw new BusinessRuleException("Cannot move an occupied bed to UNDER_MAINTENANCE or INACTIVE.");

        existing.BedNumber = bed.BedNumber;
        existing.Status = normalizedStatus;
        existing.StudentId = bed.StudentId;
        existing.StudentName = bed.StudentName;
        existing.StudentCode = bed.StudentCode;
        ApplyStudentOccupancyFields(existing);
        existing.UpdatedAt = DateTime.UtcNow;

        await SyncRoomByBedsAsync(existing.Room);
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await db.Beds
            .Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.Room).ThenInclude(r => r.Beds)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException("Bed", id);

        if (existing.Status == "OCCUPIED")
            throw new BusinessRuleException("Cannot delete an occupied bed.");

        var room = existing.Room;
        db.Beds.Remove(existing);
        room.Beds.Remove(existing);

        await SyncRoomByBedsAsync(room);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await db.Beds.AnyAsync(b => b.Id == id);

    private async Task SyncRoomByBedsAsync(Room room)
    {
        var previousStatus = room.Status;
        var occupancy = room.Beds.Count(b => b.Status == "OCCUPIED");
        var capacity = room.RoomType?.Capacity ?? room.Beds.Count;

        room.CurrentOccupancy = occupancy;

        if (room.Status != "UNDER_MAINTENANCE" && room.Status != "INACTIVE")
            room.Status = occupancy >= capacity ? "FULL" : "AVAILABLE";

        room.UpdatedAt = DateTime.UtcNow;

        if (!string.Equals(previousStatus, room.Status, StringComparison.Ordinal))
        {
            await db.OutboxEvents.AddAsync(new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = "room.status.changed",
                AggregateId = room.Id,
                Payload = JsonSerializer.Serialize(new
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
                }),
                Status = "PENDING",
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
            });
        }
    }

    private static string NormalizeBedStatus(string? status, string fallback)
    {
        if (string.IsNullOrWhiteSpace(status)) return fallback;

        var key = status.Trim().ToUpperInvariant();
        return key switch
        {
            "AVAILABLE" => "AVAILABLE",
            "OCCUPIED" => "OCCUPIED",
            "UNDER_MAINTENANCE" => "UNDER_MAINTENANCE",
            "INACTIVE" => "INACTIVE",
            _ => throw new BusinessRuleException("Bed status must be AVAILABLE/OCCUPIED/UNDER_MAINTENANCE/INACTIVE."),
        };
    }

    private static void ApplyStudentOccupancyFields(Bed bed)
    {
        if (!string.Equals(bed.Status, "OCCUPIED", StringComparison.Ordinal))
        {
            bed.StudentId = null;
            bed.StudentName = null;
            bed.StudentCode = null;
        }
    }
}
