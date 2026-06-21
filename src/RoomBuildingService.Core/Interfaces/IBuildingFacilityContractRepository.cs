namespace RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Core.Entities;
public interface IBuildingFacilityContractRepository
{
    Task<IEnumerable<BuildingFacilityContract>> GetAllAsync(Guid? facilityId, Guid? pricingId, string? status);
    Task<BuildingFacilityContract?> GetByIdAsync(Guid id);
    Task<BuildingFacilityContract> CreateAsync(BuildingFacilityContract contract);
    Task<BuildingFacilityContract> UpdateAsync(BuildingFacilityContract contract);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
