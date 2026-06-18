namespace RoomBuildingService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Api.DTOs.Bed;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Interfaces;
[ApiController]
[Route("api/[controller]")]
public class BedsController(IBedRepository repo) : ControllerBase
{
// GET /api/beds?roomId=...
[HttpGet]
public async Task<IActionResult> GetByRoom([FromQuery] Guid roomId)
{
var beds = await repo.GetByRoomAsync(roomId);
return Ok(beds.Select(b => new BedResponse
{
Id        = b.Id,
RoomId    = b.RoomId,
BedNumber = b.BedNumber,
Status    = b.Status,
CreatedAt = b.CreatedAt,
UpdatedAt = b.UpdatedAt
}));
}
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var bed = await repo.GetByIdAsync(id);
    if (bed is null) return NotFound();
    return Ok(new BedResponse
    {
        Id        = bed.Id,
        RoomId    = bed.RoomId,
        BedNumber = bed.BedNumber,
        Status    = bed.Status,
        CreatedAt = bed.CreatedAt,
        UpdatedAt = bed.UpdatedAt
    });
}

[HttpPost]
public async Task<IActionResult> Create([FromBody] BedCreateRequest req)
{
    var bed = new Bed { RoomId = req.RoomId, BedNumber = req.BedNumber };
    var created = await repo.CreateAsync(bed);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, new BedResponse
    {
        Id        = created.Id,
        RoomId    = created.RoomId,
        BedNumber = created.BedNumber,
        Status    = created.Status,
        CreatedAt = created.CreatedAt,
        UpdatedAt = created.UpdatedAt
    });
}

[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] BedUpdateRequest req)
{
    var bed = new Bed { Id = id, BedNumber = req.BedNumber, Status = req.Status };
    var updated = await repo.UpdateAsync(bed);
    return Ok(new BedResponse
    {
        Id        = updated.Id,
        RoomId    = updated.RoomId,
        BedNumber = updated.BedNumber,
        Status    = updated.Status,
        CreatedAt = updated.CreatedAt,
        UpdatedAt = updated.UpdatedAt
    });
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)
{
    await repo.DeleteAsync(id);
    return NoContent();
}
}