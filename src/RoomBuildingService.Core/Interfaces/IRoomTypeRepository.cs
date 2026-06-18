namespace RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Core.Entities;
    public interface IRoomTypeRepository
        {
            Task<IEnumerable<RoomType>> GetAllAsync();
            Task<RoomType?>             GetByIdAsync(Guid id);       // Include Amenities
            Task<RoomType>              CreateAsync(RoomType roomType);
            Task<RoomType>              UpdateAsync(RoomType roomType);
            Task                        DeleteAsync(Guid id);
            Task<bool>                  ExistsAsync(Guid id);
        }