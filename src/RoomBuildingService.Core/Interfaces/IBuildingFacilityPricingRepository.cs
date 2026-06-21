namespace RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Core.Entities;
public interface IBuildingFacilityPricingRepository
{
    Task<IEnumerable<BuildingFacilityPricing>> GetAllAsync(Guid? facilityId);
    Task<BuildingFacilityPricing?> GetByIdAsync(Guid id);
    Task<BuildingFacilityPricing> CreateAsync(BuildingFacilityPricing pricing);
    Task<BuildingFacilityPricing> UpdateAsync(BuildingFacilityPricing pricing);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
