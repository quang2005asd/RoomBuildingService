namespace RoomBuildingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;
public class BuildingFacilityRepository(AppDbContext db) : IBuildingFacilityRepository
{
    public async Task<IEnumerable<BuildingFacility>> GetAllAsync(Guid? buildingId)
    {
        var query = db.BuildingFacilities.AsNoTracking().AsQueryable();
        if (buildingId.HasValue) query = query.Where(x => x.BuildingId == buildingId.Value);
        return await query.OrderBy(x => x.BuildingId).ThenBy(x => x.FacilityName).ToListAsync();
    }

    public async Task<BuildingFacility?> GetByIdAsync(Guid id)
        => await db.BuildingFacilities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<BuildingFacility> CreateAsync(BuildingFacility facility)
    {
        if (!await db.Buildings.AnyAsync(x => x.Id == facility.BuildingId))
            throw new NotFoundException("Building", facility.BuildingId);

        if (await db.BuildingFacilities.AnyAsync(x => x.BuildingId == facility.BuildingId && x.FacilityName == facility.FacilityName))
            throw new BusinessRuleException($"Facility '{facility.FacilityName}' already exists in this building.");

        facility.Id = Guid.NewGuid();
        facility.Status = NormalizeStatus(facility.Status, "ACTIVE");
        facility.CreatedAt = DateTime.UtcNow;
        db.BuildingFacilities.Add(facility);
        await db.SaveChangesAsync();
        return facility;
    }

    public async Task<BuildingFacility> UpdateAsync(BuildingFacility facility)
    {
        var existing = await db.BuildingFacilities.FirstOrDefaultAsync(x => x.Id == facility.Id)
            ?? throw new NotFoundException("BuildingFacility", facility.Id);

        existing.FacilityName = facility.FacilityName;
        existing.Status = NormalizeStatus(facility.Status, existing.Status);
        existing.Description = facility.Description;
        existing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await db.BuildingFacilities
            .Include(x => x.Pricings)
            .Include(x => x.Contracts)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("BuildingFacility", id);

        if (existing.Pricings.Any())
            throw new BusinessRuleException("Cannot delete a facility that still has pricing policies.");

        if (existing.Contracts.Any())
            throw new BusinessRuleException("Cannot delete a facility that still has stored contracts.");

        db.BuildingFacilities.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id) => await db.BuildingFacilities.AnyAsync(x => x.Id == id);

    private static string NormalizeStatus(string? status, string fallback)
    {
        if (string.IsNullOrWhiteSpace(status)) return fallback;
        var key = status.Trim().ToUpperInvariant();
        return key switch
        {
            "ACTIVE" => "ACTIVE",
            "INACTIVE" => "INACTIVE",
            "UNDER_MAINTENANCE" => "UNDER_MAINTENANCE",
            _ => throw new BusinessRuleException("Facility status must be ACTIVE/INACTIVE/UNDER_MAINTENANCE.")
        };
    }
}
