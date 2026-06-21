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
        var existing = await db.Buildings
            .Include(b => b.Rooms)
            .ThenInclude(r => r.Beds)
            .FirstOrDefaultAsync(b => b.Id == building.Id)
            ?? throw new NotFoundException("Building", building.Id);

        var normalizedStatus = NormalizeBuildingStatus(building.Status, existing.Status);
        var hasResidents = existing.Rooms.Any(r => r.Beds.Any(b => b.Status == "OCCUPIED"));

        if (hasResidents && normalizedStatus is "INACTIVE" or "UNDER_MAINTENANCE")
            throw new BusinessRuleException("Cannot move a building to INACTIVE or UNDER_MAINTENANCE while it still has residents.");

        existing.Name        = building.Name;
        existing.TotalFloors = building.TotalFloors;
        existing.GenderType  = building.GenderType;
        existing.Status      = normalizedStatus;
        existing.Description = building.Description;
        existing.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await db.Buildings
            .Include(b => b.Rooms).ThenInclude(r => r.Beds)
            .Include(b => b.Facilities)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException("Building", id);

        if (existing.Rooms.Any(r => r.Beds.Any(b => b.Status == "OCCUPIED")))
            throw new BusinessRuleException("Cannot delete a building that still has residents in its rooms.");

        if (existing.Rooms.Any())
            throw new BusinessRuleException("Cannot delete a building that still has rooms. Delete rooms first.");

        if (existing.Facilities.Any())
            throw new BusinessRuleException("Cannot delete a building that still has facilities. Delete facility records first.");

        db.Buildings.Remove(existing);
        await db.SaveChangesAsync();
    }

public async Task<bool> ExistsAsync(Guid id)
    => await db.Buildings.AnyAsync(b => b.Id == id);

    private static string NormalizeBuildingStatus(string? status, string fallback)
    {
        if (string.IsNullOrWhiteSpace(status))
            return fallback;

        var key = status.Trim().ToUpperInvariant();
        return key switch
        {
            "ACTIVE" => "ACTIVE",
            "INACTIVE" => "INACTIVE",
            "UNDER_MAINTENANCE" => "UNDER_MAINTENANCE",
            _ => throw new BusinessRuleException("Building status must be ACTIVE/INACTIVE/UNDER_MAINTENANCE.")
        };
    }
}
