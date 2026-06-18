namespace RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Core.Entities;
public interface IRoomRepository
    {
        // Lấy danh sách — có filter theo tòa, tầng, trạng thái
        Task<IEnumerable<Room>> GetAllAsync(Guid? buildingId, int? floor, string? status);
        // Lấy theo tầng — phục vụ API sơ đồ phòng
        Task<IEnumerable<Room>> GetByFloorAsync(Guid buildingId, int floor);
        Task<IEnumerable<Room>> GetAvailableAsync(Guid? buildingId, Guid? roomTypeId, string? genderType, DateOnly? expectedStartDate, DateOnly? expectedEndDate);
        Task<Room?>  GetByIdAsync(Guid id);          // Include RoomType + Beds + Equipments
        Task<Room>   CreateAsync(Room room);
        Task<Room>   UpdateAsync(Room room);
        Task         UpdateStatusAsync(Guid id, string status, string? maintenanceReason);
        Task         DeleteAsync(Guid id);
        Task<bool>   ExistsAsync(Guid id);
    }
