namespace RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Core.Entities;
public interface IBuildingFacilityRepository
{
    Task<IEnumerable<BuildingFacility>> GetAllAsync(Guid? buildingId);
    Task<BuildingFacility?> GetByIdAsync(Guid id);
    Task<BuildingFacility> CreateAsync(BuildingFacility facility);
    Task<BuildingFacility> UpdateAsync(BuildingFacility facility);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
