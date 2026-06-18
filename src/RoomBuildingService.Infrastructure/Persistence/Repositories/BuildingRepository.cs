namespace RoomBuildingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;
public class BuildingRepository(AppDbContext db) : IBuildingRepository
{
    public async Task<IEnumerable<Building>> GetAllAsync()
    => await db.Buildings.AsNoTracking().ToListAsync();
    public async Task<Building?> GetByIdAsync(Guid id)
        => await db.Buildings.AsNoTracking()
                .Include(b => b.Rooms)
                .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Building> CreateAsync(Building building)
    {
        building.Id        = Guid.NewGuid();
        building.CreatedAt = DateTime.UtcNow;
        db.Buildings.Add(building);
        await db.SaveChangesAsync();
        return building;
    }

    public async Task<Building> UpdateAsync(Building building)
    {
        var existing = await db.Buildings.FindAsync(building.Id)
            ?? throw new NotFoundException("Building", building.Id);

        existing.Name        = building.Name;
        existing.TotalFloors = building.TotalFloors;
        existing.GenderType  = building.GenderType;
        existing.Status      = building.Status;
        existing.Description = building.Description;
        existing.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await db.Buildings.FindAsync(id)
            ?? throw new NotFoundException("Building", id);

        db.Buildings.Remove(existing);
        await db.SaveChangesAsync();
    }

public async Task<bool> ExistsAsync(Guid id)
    => await db.Buildings.AnyAsync(b => b.Id == id);
}