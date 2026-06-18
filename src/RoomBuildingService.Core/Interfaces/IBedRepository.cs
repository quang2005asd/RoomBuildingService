namespace RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Core.Entities;
public interface IBedRepository
    {
        Task<IEnumerable<Bed>> GetByRoomAsync(Guid roomId);
        Task<Bed?>             GetByIdAsync(Guid id);
        Task<Bed>              CreateAsync(Bed bed);
        Task<Bed>              UpdateAsync(Bed bed);
        Task                   DeleteAsync(Guid id);
        Task<bool>             ExistsAsync(Guid id);
    }