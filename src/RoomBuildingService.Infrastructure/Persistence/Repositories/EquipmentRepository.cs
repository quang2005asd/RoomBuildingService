namespace RoomBuildingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;
public class EquipmentRepository(AppDbContext db) : IEquipmentRepository
{
public async Task<IEnumerable<RoomEquipment>> GetByRoomAsync(Guid roomId)
=> await db.RoomEquipments.AsNoTracking()
.Where(e => e.RoomId == roomId)
.OrderBy(e => e.EquipmentName)
.ThenBy(e => e.EquipmentIndex)
.ToListAsync();
public async Task<RoomEquipment?> GetByIdAsync(Guid id)
    => await db.RoomEquipments.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

public async Task<RoomEquipment> CreateAsync(RoomEquipment equipment)
{
    if (!await db.Rooms.AnyAsync(r => r.Id == equipment.RoomId))
        throw new NotFoundException("Room", equipment.RoomId);

    // Tự động tính EquipmentIndex nếu đã có thiết bị cùng tên
    var maxIndex = await db.RoomEquipments
        .Where(e => e.RoomId == equipment.RoomId && e.EquipmentName == equipment.EquipmentName)
        .MaxAsync(e => (short?)e.EquipmentIndex) ?? 0;

    equipment.Id             = Guid.NewGuid();
    equipment.EquipmentIndex = (short)(maxIndex + 1);
    equipment.CreatedAt      = DateTime.UtcNow;
    equipment.Status         = "ACTIVE";

    db.RoomEquipments.Add(equipment);
    await db.SaveChangesAsync();
    return equipment;
}

public async Task UpdateStatusAsync(Guid id, string status)
{
    var existing = await db.RoomEquipments.FindAsync(id)
        ?? throw new NotFoundException("RoomEquipment", id);

    existing.Status    = status;
    existing.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
}

public async Task DeleteAsync(Guid id)
{
    var existing = await db.RoomEquipments.FindAsync(id)
        ?? throw new NotFoundException("RoomEquipment", id);

    db.RoomEquipments.Remove(existing);
    await db.SaveChangesAsync();
}

public async Task<bool> ExistsAsync(Guid id)
    => await db.RoomEquipments.AnyAsync(e => e.Id == id);
}