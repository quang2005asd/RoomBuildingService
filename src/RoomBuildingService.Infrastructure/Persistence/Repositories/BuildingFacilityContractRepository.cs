namespace RoomBuildingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;
public class BuildingFacilityContractRepository(AppDbContext db) : IBuildingFacilityContractRepository
{
    public async Task<IEnumerable<BuildingFacilityContract>> GetAllAsync(Guid? facilityId, Guid? pricingId, string? status)
    {
        var query = db.BuildingFacilityContracts.AsNoTracking().AsQueryable();
        if (facilityId.HasValue) query = query.Where(x => x.FacilityId == facilityId.Value);
        if (pricingId.HasValue) query = query.Where(x => x.PricingId == pricingId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status.Trim().ToUpperInvariant());
        return await query.OrderByDescending(x => x.StartDate).ThenBy(x => x.ContractCode).ToListAsync();
    }

    public async Task<BuildingFacilityContract?> GetByIdAsync(Guid id)
        => await db.BuildingFacilityContracts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<BuildingFacilityContract> CreateAsync(BuildingFacilityContract contract)
    {
        await EnsureRelationsAsync(contract.FacilityId, contract.PricingId);
        NormalizeContract(contract);
        contract.Id = Guid.NewGuid();
        contract.CreatedAt = DateTime.UtcNow;
        db.BuildingFacilityContracts.Add(contract);
        await db.SaveChangesAsync();
        return contract;
    }

    public async Task<BuildingFacilityContract> UpdateAsync(BuildingFacilityContract contract)
    {
        var existing = await db.BuildingFacilityContracts.FirstOrDefaultAsync(x => x.Id == contract.Id)
            ?? throw new NotFoundException("BuildingFacilityContract", contract.Id);

        existing.StudentId = contract.StudentId;
        existing.StudentName = contract.StudentName;
        existing.StudentCode = contract.StudentCode;
        existing.ContractType = contract.ContractType;
        existing.StartDate = contract.StartDate;
        existing.EndDate = contract.EndDate;
        existing.TotalAmount = contract.TotalAmount;
        existing.Status = contract.Status;
        existing.Notes = contract.Notes;
        NormalizeContract(existing);
        existing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await db.BuildingFacilityContracts.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("BuildingFacilityContract", id);

        if (existing.Status == "ACTIVE")
            throw new BusinessRuleException("Cannot delete an active facility contract. Cancel or expire it first.");

        db.BuildingFacilityContracts.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id) => await db.BuildingFacilityContracts.AnyAsync(x => x.Id == id);

    private async Task EnsureRelationsAsync(Guid facilityId, Guid pricingId)
    {
        if (!await db.BuildingFacilities.AnyAsync(x => x.Id == facilityId))
            throw new NotFoundException("BuildingFacility", facilityId);

        var pricing = await db.BuildingFacilityPricings.FirstOrDefaultAsync(x => x.Id == pricingId)
            ?? throw new NotFoundException("BuildingFacilityPricing", pricingId);

        if (pricing.FacilityId != facilityId)
            throw new BusinessRuleException("Pricing policy does not belong to the selected facility.");

        if (!pricing.IsPaid)
            throw new BusinessRuleException("Only paid pricing policies can create facility contracts.");
    }

    private static void NormalizeContract(BuildingFacilityContract contract)
    {
        contract.ContractCode = contract.ContractCode.Trim();
        contract.StudentName = string.IsNullOrWhiteSpace(contract.StudentName) ? null : contract.StudentName.Trim();
        contract.StudentCode = string.IsNullOrWhiteSpace(contract.StudentCode) ? null : contract.StudentCode.Trim();
        contract.Notes = string.IsNullOrWhiteSpace(contract.Notes) ? null : contract.Notes.Trim();
        contract.ContractType = NormalizeType(contract.ContractType);
        contract.Status = NormalizeStatus(contract.Status);

        if (contract.StartDate > contract.EndDate)
            throw new BusinessRuleException("StartDate must be earlier than or equal to EndDate.");

        if (contract.TotalAmount < 0)
            throw new BusinessRuleException("TotalAmount must be greater than or equal to 0.");
    }

    private static string NormalizeType(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "MONTHLY";
        var key = input.Trim().ToUpperInvariant();
        return key switch
        {
            "MONTHLY" => "MONTHLY",
            "YEARLY" => "YEARLY",
            _ => throw new BusinessRuleException("ContractType must be MONTHLY/YEARLY.")
        };
    }

    private static string NormalizeStatus(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "ACTIVE";
        var key = input.Trim().ToUpperInvariant();
        return key switch
        {
            "ACTIVE" => "ACTIVE",
            "EXPIRED" => "EXPIRED",
            "CANCELLED" => "CANCELLED",
            _ => throw new BusinessRuleException("Contract status must be ACTIVE/EXPIRED/CANCELLED.")
        };
    }
}
