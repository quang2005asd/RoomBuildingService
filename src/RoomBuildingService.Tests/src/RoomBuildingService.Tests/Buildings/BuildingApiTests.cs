namespace RoomBuildingService.Tests.Buildings;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
public class BuildingApiTests : IClassFixture<CustomWebAppFactory>
{
private readonly HttpClient _client;
public BuildingApiTests(CustomWebAppFactory factory)
{
    _client = factory.CreateClient();
}

// ── GET /api/buildings ──────────────────────────────────
[Fact]
public async Task GetAll_ShouldReturn200_WithEmptyList()
{
    var res = await _client.GetAsync("/api/buildings");

    res.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await res.Content.ReadFromJsonAsync<List<object>>();
    body.Should().NotBeNull();
}

// ── POST /api/buildings ─────────────────────────────────
[Fact]
public async Task Create_ValidRequest_ShouldReturn201()
{
    var req = new
    {
        name        = "KTX A",
        totalFloors = 6,
        genderType  = "MALE",
        description = "Khu A nam"
    };

    var res = await _client.PostAsJsonAsync("/api/buildings", req);

    res.StatusCode.Should().Be(HttpStatusCode.Created);
    var body = await res.Content.ReadFromJsonAsync<BuildingDto>();
    body!.Name.Should().Be("KTX A");
    body.GenderType.Should().Be("MALE");
    body.Status.Should().Be("ACTIVE");
}

[Fact]
public async Task Create_MissingName_ShouldReturn400()
{
    var req = new { totalFloors = 6, genderType = "MALE" }; // Thiếu name

    var res = await _client.PostAsJsonAsync("/api/buildings", req);

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

[Fact]
public async Task Create_InvalidGenderType_ShouldReturn400()
{
    var req = new { name = "KTX A", totalFloors = 6, genderType = "INVALID" };

    var res = await _client.PostAsJsonAsync("/api/buildings", req);

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

// ── GET /api/buildings/{id} ─────────────────────────────
[Fact]
public async Task GetById_ExistingId_ShouldReturn200()
{
    // Tạo trước
    var created = await CreateBuildingAsync("KTX B", "FEMALE");

    var res = await _client.GetAsync($"/api/buildings/{created.Id}");

    res.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await res.Content.ReadFromJsonAsync<BuildingDto>();
    body!.Name.Should().Be("KTX B");
}

[Fact]
public async Task GetById_NotExist_ShouldReturn404()
{
    var res = await _client.GetAsync($"/api/buildings/{Guid.NewGuid()}");

    res.StatusCode.Should().Be(HttpStatusCode.NotFound);
}

// ── PUT /api/buildings/{id} ─────────────────────────────
[Fact]
public async Task Update_ValidRequest_ShouldReturn200()
{
    var created = await CreateBuildingAsync("KTX C", "MIXED");

    var updateReq = new
    {
        name        = "KTX C Updated",
        totalFloors = 8,
        genderType  = "MIXED",
        status      = "ACTIVE"
    };

    var res = await _client.PutAsJsonAsync($"/api/buildings/{created.Id}", updateReq);

    res.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await res.Content.ReadFromJsonAsync<BuildingDto>();
    body!.Name.Should().Be("KTX C Updated");
    body.TotalFloors.Should().Be(8);
}

// ── DELETE /api/buildings/{id} ──────────────────────────
[Fact]
public async Task Delete_ExistingId_ShouldReturn204()
{
    var created = await CreateBuildingAsync("KTX D", "MALE");

    var res = await _client.DeleteAsync($"/api/buildings/{created.Id}");

    res.StatusCode.Should().Be(HttpStatusCode.NoContent);
}

[Fact]
public async Task Delete_NotExist_ShouldReturn404()
{
    var res = await _client.DeleteAsync($"/api/buildings/{Guid.NewGuid()}");

    res.StatusCode.Should().Be(HttpStatusCode.NotFound);
}

// ── Helper ──────────────────────────────────────────────
private async Task<BuildingDto> CreateBuildingAsync(string name, string genderType)
{
    var req = new { name, totalFloors = 5, genderType };
    var res = await _client.PostAsJsonAsync("/api/buildings", req);
    return (await res.Content.ReadFromJsonAsync<BuildingDto>())!;
}
}
public record BuildingDto(Guid Id, string Name, int TotalFloors, string GenderType, string Status);