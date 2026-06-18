namespace RoomBuildingService.Tests.Rooms;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
public class RoomApiTests : IClassFixture<CustomWebAppFactory>
{
private readonly HttpClient _client;
public RoomApiTests(CustomWebAppFactory factory)
{
    _client = factory.CreateClient();
}

// ── POST /api/rooms ─────────────────────────────────────
[Fact]
public async Task Create_ValidRequest_ShouldReturn201()
{
    var building = await CreateBuildingAsync();
    var roomType = await CreateRoomTypeAsync();

    var req = new
    {
        buildingId  = building.Id,
        roomTypeId  = roomType.Id,
        roomNumber  = "A101",
        floorNumber = 1
    };

    var res = await _client.PostAsJsonAsync("/api/rooms", req);

    res.StatusCode.Should().Be(HttpStatusCode.Created);
    var body = await res.Content.ReadFromJsonAsync<RoomDto>();
    body!.RoomNumber.Should().Be("A101");
    body.Status.Should().Be("AVAILABLE");
}

[Fact]
public async Task Create_FloorExceedsTotalFloors_ShouldReturn400()
{
    var building = await CreateBuildingAsync(totalFloors: 3);
    var roomType = await CreateRoomTypeAsync();

    var req = new
    {
        buildingId  = building.Id,
        roomTypeId  = roomType.Id,
        roomNumber  = "A401",
        floorNumber = 4  // Vượt quá totalFloors = 3
    };

    var res = await _client.PostAsJsonAsync("/api/rooms", req);

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

[Fact]
public async Task Create_DuplicateRoomNumber_ShouldReturn400()
{
    var building = await CreateBuildingAsync();
    var roomType = await CreateRoomTypeAsync();

    var req = new { buildingId = building.Id, roomTypeId = roomType.Id, roomNumber = "A101", floorNumber = 1 };

    await _client.PostAsJsonAsync("/api/rooms", req); // Tạo lần 1
    var res = await _client.PostAsJsonAsync("/api/rooms", req); // Tạo lần 2 — trùng

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

// ── GET /api/rooms?status=AVAILABLE ─────────────────────
[Fact]
public async Task GetAll_FilterByStatus_ShouldReturnFiltered()
{
    var building = await CreateBuildingAsync();
    var roomType = await CreateRoomTypeAsync();

    await CreateRoomAsync(building.Id, roomType.Id, "B101");
    await CreateRoomAsync(building.Id, roomType.Id, "B102");

    var res = await _client.GetAsync("/api/rooms?status=AVAILABLE");

    res.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await res.Content.ReadFromJsonAsync<List<RoomDto>>();
    body!.Should().OnlyContain(r => r.Status == "AVAILABLE");
}

// ── PATCH /api/rooms/{id}/status ────────────────────────
[Fact]
public async Task UpdateStatus_ToMaintenance_WithReason_ShouldReturn204()
{
    var building = await CreateBuildingAsync();
    var roomType = await CreateRoomTypeAsync();
    var room     = await CreateRoomAsync(building.Id, roomType.Id, "C101");

    var req = new { status = "UNDER_MAINTENANCE", maintenanceReason = "Sua dieu hoa" };
    var res = await _client.PatchAsJsonAsync($"/api/rooms/{room.Id}/status", req);

    res.StatusCode.Should().Be(HttpStatusCode.NoContent);
}

[Fact]
public async Task UpdateStatus_ToMaintenance_WithoutReason_ShouldReturn400()
{
    var building = await CreateBuildingAsync();
    var roomType = await CreateRoomTypeAsync();
    var room     = await CreateRoomAsync(building.Id, roomType.Id, "C102");

    var req = new { status = "UNDER_MAINTENANCE" }; // Thiếu maintenanceReason
    var res = await _client.PatchAsJsonAsync($"/api/rooms/{room.Id}/status", req);

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

// ── GET /api/rooms/floormap ──────────────────────────────
[Fact]
public async Task GetFloorMap_ShouldReturnRoomsOnFloor()
{
    var building = await CreateBuildingAsync();
    var roomType = await CreateRoomTypeAsync();

    await CreateRoomAsync(building.Id, roomType.Id, "D101", floor: 1);
    await CreateRoomAsync(building.Id, roomType.Id, "D102", floor: 1);
    await CreateRoomAsync(building.Id, roomType.Id, "D201", floor: 2);

    var res = await _client.GetAsync($"/api/rooms/floormap?buildingId={building.Id}&floor=1");

    res.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await res.Content.ReadFromJsonAsync<List<RoomDto>>();
    body!.Should().HaveCount(2);
    body.Should().OnlyContain(r => r.FloorNumber == 1);
}

// ── Helpers ─────────────────────────────────────────────
private async Task<BuildingDto> CreateBuildingAsync(int totalFloors = 10)
{
    var req = new { name = "KTX Test " + Guid.NewGuid(), totalFloors, genderType = "MIXED" };
    var res = await _client.PostAsJsonAsync("/api/buildings", req);
    return (await res.Content.ReadFromJsonAsync<BuildingDto>())!;
}

private async Task<RoomTypeDto> CreateRoomTypeAsync()
{
    var req = new { typeName = "Loai " + Guid.NewGuid(), capacity = 6, basePrice = 1200000 };
    var res = await _client.PostAsJsonAsync("/api/roomtypes", req);
    return (await res.Content.ReadFromJsonAsync<RoomTypeDto>())!;
}

private async Task<RoomDto> CreateRoomAsync(Guid buildingId, Guid roomTypeId, string roomNumber, int floor = 1)
{
    var req = new { buildingId, roomTypeId, roomNumber, floorNumber = floor };
    var res = await _client.PostAsJsonAsync("/api/rooms", req);
    return (await res.Content.ReadFromJsonAsync<RoomDto>())!;
}
}
public record RoomDto(Guid Id, string RoomNumber, int FloorNumber, string Status, int CurrentOccupancy);
public record RoomTypeDto(Guid Id, string TypeName, int Capacity, decimal BasePrice);
public record BuildingDto(Guid Id);