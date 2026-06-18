namespace RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Core.Entities;
public interface IEquipmentRepository
    {
        Task<IEnumerable<RoomEquipment>> GetByRoomAsync(Guid roomId);
        Task<RoomEquipment?>             GetByIdAsync(Guid id);
        Task<RoomEquipment>              CreateAsync(RoomEquipment equipment);
        Task                             UpdateStatusAsync(Guid id, string status);  // Nhóm Maintenance gọi
        Task                             DeleteAsync(Guid id);
        Task<bool>                       ExistsAsync(Guid id);
    }