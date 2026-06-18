using RoomBuildingService.Core.Entities;
using RoomBuildingService.Infrastructure.Persistence;

namespace RoomBuildingService.Api;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Buildings.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var buildingA = new Building
        {
            Id = Guid.NewGuid(),
            Name = "KTX A",
            TotalFloors = 6,
            GenderType = "MALE",
            Status = "ACTIVE",
            Description = "Khu A danh cho nam sinh vien",
            CreatedAt = now,
        };

        var buildingB = new Building
        {
            Id = Guid.NewGuid(),
            Name = "KTX B",
            TotalFloors = 5,
            GenderType = "FEMALE",
            Status = "ACTIVE",
            Description = "Khu B danh cho nu sinh vien",
            CreatedAt = now,
        };

        var roomType4 = new RoomType
        {
            Id = Guid.NewGuid(),
            TypeName = "Phong 4 nguoi",
            Capacity = 4,
            BasePrice = 1800000,
            Description = "Phong 4 nguoi co dieu hoa",
            CreatedAt = now,
            Amenities =
            [
                new RoomAmenity { Id = Guid.NewGuid(), AmenityName = "Dieu hoa", CreatedAt = now },
                new RoomAmenity { Id = Guid.NewGuid(), AmenityName = "WC rieng", CreatedAt = now },
                new RoomAmenity { Id = Guid.NewGuid(), AmenityName = "Ban hoc", CreatedAt = now },
            ],
        };

        var roomType6 = new RoomType
        {
            Id = Guid.NewGuid(),
            TypeName = "Phong 6 nguoi",
            Capacity = 6,
            BasePrice = 1200000,
            Description = "Phong tieu chuan 6 nguoi",
            CreatedAt = now,
            Amenities =
            [
                new RoomAmenity { Id = Guid.NewGuid(), AmenityName = "Quat tran", CreatedAt = now },
                new RoomAmenity { Id = Guid.NewGuid(), AmenityName = "Tu ca nhan", CreatedAt = now },
            ],
        };

        var roomA101 = CreateRoom(
            buildingA.Id,
            roomType6,
            "A101",
            1,
            4,
            "AVAILABLE",
            now,
            ["May lanh", "Quat tran"]);

        var roomA203 = CreateRoom(
            buildingA.Id,
            roomType4,
            "A203",
            2,
            4,
            "FULL",
            now,
            ["May lanh", "Tu do"]);

        var roomB205 = CreateRoom(
            buildingB.Id,
            roomType6,
            "B205",
            2,
            2,
            "AVAILABLE",
            now,
            ["Quat tran", "Den hoc"]);

        db.Buildings.AddRange(buildingA, buildingB);
        db.RoomTypes.AddRange(roomType4, roomType6);
        db.Rooms.AddRange(roomA101, roomA203, roomB205);
        db.SaveChanges();
    }

    private static Room CreateRoom(
        Guid buildingId,
        RoomType roomType,
        string roomNumber,
        int floorNumber,
        int occupiedCount,
        string status,
        DateTime now,
        IEnumerable<string> equipmentNames)
    {
        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            BuildingId = buildingId,
            RoomTypeId = roomType.Id,
            RoomType = roomType,
            RoomNumber = roomNumber,
            FloorNumber = floorNumber,
            CurrentOccupancy = occupiedCount,
            Status = status,
            CreatedAt = now,
        };

        for (var i = 1; i <= roomType.Capacity; i++)
        {
            room.Beds.Add(new Bed
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                BedNumber = $"{roomNumber}-{i:00}",
                Status = i <= occupiedCount ? "OCCUPIED" : "AVAILABLE",
                CreatedAt = now,
            });
        }

        short equipmentIndex = 1;
        foreach (var equipmentName in equipmentNames)
        {
            room.Equipments.Add(new RoomEquipment
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                EquipmentName = equipmentName,
                EquipmentIndex = equipmentIndex++,
                Status = "ACTIVE",
                CreatedAt = now,
            });
        }

        return room;
    }
}
