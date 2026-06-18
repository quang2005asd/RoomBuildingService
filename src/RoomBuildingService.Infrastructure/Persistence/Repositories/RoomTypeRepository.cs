namespace RoomBuildingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;
public class RoomTypeRepository(AppDbContext db) : IRoomTypeRepository
{
public async Task<IEnumerable<RoomType>> GetAllAsync()
=> await db.RoomTypes.AsNoTracking()
.Include(rt => rt.Amenities)
.ToListAsync();
public async Task<RoomType?> GetByIdAsync(Guid id)
    => await db.RoomTypes.AsNoTracking()
               .Include(rt => rt.Amenities)
               .FirstOrDefaultAsync(rt => rt.Id == id);

public async Task<RoomType> CreateAsync(RoomType roomType)
{
    // Kiểm tra trùng tên loại phòng
    if (await db.RoomTypes.AnyAsync(rt => rt.TypeName == roomType.TypeName))
        throw new BusinessRuleException($"Loại phòng '{roomType.TypeName}' đã tồn tại.");

    roomType.Id        = Guid.NewGuid();
    roomType.CreatedAt = DateTime.UtcNow;
    roomType.ImageUrl  = NormalizeImageUrl(roomType.ImageUrl);

    foreach (var amenity in roomType.Amenities)
    {
        amenity.Id         = Guid.NewGuid();
        amenity.RoomTypeId = roomType.Id;
        amenity.CreatedAt  = DateTime.UtcNow;
    }

    db.RoomTypes.Add(roomType);
    await db.SaveChangesAsync();
    return roomType;
}

public async Task<RoomType> UpdateAsync(RoomType roomType)
{
    var existing = await db.RoomTypes
                           .Include(rt => rt.Amenities)
                           .FirstOrDefaultAsync(rt => rt.Id == roomType.Id)
        ?? throw new NotFoundException("RoomType", roomType.Id);

    existing.TypeName    = roomType.TypeName;
    existing.Capacity    = roomType.Capacity;
    existing.BasePrice   = roomType.BasePrice;
    existing.ImageUrl    = NormalizeImageUrl(roomType.ImageUrl);
    existing.Description = roomType.Description;
    existing.UpdatedAt   = DateTime.UtcNow;

    // Đồng bộ danh sách tiện nghi để tránh lỗi Unique Index khi dùng EF Core
    // Xóa những cái không còn trong request (so sánh không phân biệt hoa thường)
    var toRemove = existing.Amenities
        .Where(ea => !roomType.Amenities.Any(ra => 
            string.Equals(ra.AmenityName.Trim(), ea.AmenityName.Trim(), StringComparison.OrdinalIgnoreCase)))
        .ToList();
    db.RoomAmenities.RemoveRange(toRemove);

    // Thêm những cái mới (chỉ thêm nếu chưa tồn tại trong danh sách cũ)
    var toAdd = roomType.Amenities
        .Where(ra => !existing.Amenities.Any(ea => 
            string.Equals(ea.AmenityName.Trim(), ra.AmenityName.Trim(), StringComparison.OrdinalIgnoreCase)))
        .Select(ra => new RoomAmenity {
            Id = Guid.NewGuid(),
            RoomTypeId = existing.Id,
            AmenityName = ra.AmenityName.Trim(),
            CreatedAt = DateTime.UtcNow
        })
        .ToList();
    
    foreach(var a in toAdd) existing.Amenities.Add(a);

    await db.SaveChangesAsync();
    return existing;
}

public async Task DeleteAsync(Guid id)
{
    var existing = await db.RoomTypes.FindAsync(id)
        ?? throw new NotFoundException("RoomType", id);

    // Không xóa nếu còn phòng đang dùng loại này
    if (await db.Rooms.AnyAsync(r => r.RoomTypeId == id))
        throw new BusinessRuleException("Không thể xóa loại phòng đang được sử dụng.");

    db.RoomTypes.Remove(existing);
    await db.SaveChangesAsync();
}

public async Task<bool> ExistsAsync(Guid id)
    => await db.RoomTypes.AnyAsync(rt => rt.Id == id);

private static string? NormalizeImageUrl(string? imageUrl)
    => string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
}
