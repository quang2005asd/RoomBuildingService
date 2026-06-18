namespace RoomBuildingService.Tests.Beds;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
public class BedApiTests : IClassFixture<CustomWebAppFactory>
{
private readonly HttpClient _client;
public BedApiTests(CustomWebAppFactory factory)
{
    _client = factory.CreateClient();
}

[Fact]
public async Task Create_ValidRequest_ShouldReturn201()
{
    var roomId = await GetRoomIdAsync();

    var req = new { roomId, bedNumber = "A101-01" };
    var res = await _client.PostAsJsonAsync("/api/beds", req);

    res.StatusCode.Should().Be(HttpStatusCode.Created);
    var body = await res.Content.ReadFromJsonAsync<BedDto>();
    body!.BedNumber.Should().Be("A101-01");
    body.Status.Should().Be("AVAILABLE");
}

[Fact]
public async Task Create_DuplicateBedNumber_ShouldReturn400()
{
    var roomId = await GetRoomIdAsync();

    var req = new { roomId, bedNumber = "A101-02" };
    await _client.PostAsJsonAsync("/api/beds", req);       // Lan 1
    var res = await _client.PostAsJsonAsync("/api/beds", req); // Lan 2 trung

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

[Fact]
public async Task Delete_OccupiedBed_ShouldReturn400()
{
    var roomId = await GetRoomIdAsync();
    var bed    = await CreateBedAsync(roomId, "A101-03");

    // Đổi sang OCCUPIED
    await _client.PutAsJsonAsync($"/api/beds/{bed.Id}",
        new { bedNumber = "A101-03", status = "OCCUPIED" });

    var res = await _client.DeleteAsync($"/api/beds/{bed.Id}");

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

// ── Helpers ─────────────────────────────────────────────
private async Task<Guid> GetRoomIdAsync()
{
    var building = await _client.PostAsJsonAsync("/api/buildings",
        new { name = "B" + Guid.NewGuid(), totalFloors = 5, genderType = "MALE" });
    var b = await building.Content.ReadFromJsonAsync<BuildingDto2>();

    var roomType = await _client.PostAsJsonAsync("/api/roomtypes",
        new { typeName = "T" + Guid.NewGuid(), capacity = 6, basePrice = 1000000 });
    var rt = await roomType.Content.ReadFromJsonAsync<RoomTypeDto2>();

    var room = await _client.PostAsJsonAsync("/api/rooms",
        new { buildingId = b!.Id, roomTypeId = rt!.Id, roomNumber = "R" + Guid.NewGuid().ToString()[..4], floorNumber = 1 });
    var r = await room.Content.ReadFromJsonAsync<RoomDto2>();
    return r!.Id;
}

private async Task<BedDto> CreateBedAsync(Guid roomId, string bedNumber)
{
    var res = await _client.PostAsJsonAsync("/api/beds", new { roomId, bedNumber });
    return (await res.Content.ReadFromJsonAsync<BedDto>())!;
}
}
public record BedDto(Guid Id, Guid RoomId, string BedNumber, string Status);
public record BuildingDto2(Guid Id);
public record RoomTypeDto2(Guid Id);
public record RoomDto2(Guid Id);