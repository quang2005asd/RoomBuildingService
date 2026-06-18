namespace RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Core.Entities;
    public interface IBuildingRepository
        {
            Task<IEnumerable<Building>> GetAllAsync();
            Task<Building?>             GetByIdAsync(Guid id);
            Task<Building>              CreateAsync(Building building);
            Task<Building>              UpdateAsync(Building building);
            Task                        DeleteAsync(Guid id);
            Task<bool>                  ExistsAsync(Guid id);
        }