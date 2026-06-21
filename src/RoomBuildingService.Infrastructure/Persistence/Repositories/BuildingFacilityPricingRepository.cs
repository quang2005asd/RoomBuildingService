namespace RoomBuildingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;
public class BuildingFacilityPricingRepository(AppDbContext db) : IBuildingFacilityPricingRepository
{
    public async Task<IEnumerable<BuildingFacilityPricing>> GetAllAsync(Guid? facilityId)
    {
        var query = db.BuildingFacilityPricings.AsNoTracking().AsQueryable();
        if (facilityId.HasValue) query = query.Where(x => x.FacilityId == facilityId.Value);
        return await query.OrderBy(x => x.FacilityId).ThenBy(x => x.BillingCycle).ToListAsync();
    }

    public async Task<BuildingFacilityPricing?> GetByIdAsync(Guid id)
        => await db.BuildingFacilityPricings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<BuildingFacilityPricing> CreateAsync(BuildingFacilityPricing pricing)
    {
        if (!await db.BuildingFacilities.AnyAsync(x => x.Id == pricing.FacilityId))
            throw new NotFoundException("BuildingFacility", pricing.FacilityId);

        NormalizePricing(pricing);
        pricing.Id = Guid.NewGuid();
        pricing.CreatedAt = DateTime.UtcNow;
        db.BuildingFacilityPricings.Add(pricing);
        await db.SaveChangesAsync();
        return pricing;
    }

    public async Task<BuildingFacilityPricing> UpdateAsync(BuildingFacilityPricing pricing)
    {
        var existing = await db.BuildingFacilityPricings.FirstOrDefaultAsync(x => x.Id == pricing.Id)
            ?? throw new NotFoundException("BuildingFacilityPricing", pricing.Id);

        existing.IsPaid = pricing.IsPaid;
        existing.Price = pricing.Price;
        existing.BillingCycle = pricing.BillingCycle;
        existing.Status = pricing.Status;
        existing.Description = pricing.Description;
        NormalizePricing(existing);
        existing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await db.BuildingFacilityPricings
            .Include(x => x.Contracts)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("BuildingFacilityPricing", id);

        if (existing.Contracts.Any())
            throw new BusinessRuleException("Cannot delete a pricing policy that still has contracts.");

        db.BuildingFacilityPricings.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id) => await db.BuildingFacilityPricings.AnyAsync(x => x.Id == id);

    private static void NormalizePricing(BuildingFacilityPricing pricing)
    {
        pricing.BillingCycle = NormalizeCycle(pricing.BillingCycle);
        pricing.Status = NormalizeStatus(pricing.Status);
        pricing.Description = string.IsNullOrWhiteSpace(pricing.Description) ? null : pricing.Description.Trim();

        if (!pricing.IsPaid)
        {
            pricing.Price = 0;
            pricing.BillingCycle = "FREE";
        }
    }

    private static string NormalizeCycle(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "FREE";
        var key = input.Trim().ToUpperInvariant();
        return key switch
        {
            "FREE" => "FREE",
            "MONTHLY" => "MONTHLY",
            "YEARLY" => "YEARLY",
            _ => throw new BusinessRuleException("BillingCycle must be FREE/MONTHLY/YEARLY.")
        };
    }

    private static string NormalizeStatus(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "ACTIVE";
        var key = input.Trim().ToUpperInvariant();
        return key switch
        {
            "ACTIVE" => "ACTIVE",
            "INACTIVE" => "INACTIVE",
            _ => throw new BusinessRuleException("Pricing status must be ACTIVE/INACTIVE.")
        };
    }
}
