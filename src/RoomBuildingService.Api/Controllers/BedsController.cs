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
return Ok(beds.Select(MapBedResponse));
}
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var bed = await repo.GetByIdAsync(id);
    if (bed is null) return NotFound();
    return Ok(MapBedResponse(bed));
}

[HttpPost]
public async Task<IActionResult> Create([FromBody] BedCreateRequest req)
{
    var bed = new Bed
    {
        RoomId = req.RoomId,
        BedNumber = req.BedNumber,
        StudentId = req.StudentId,
        StudentName = req.StudentName,
        StudentCode = req.StudentCode,
        Status = req.StudentId.HasValue || !string.IsNullOrWhiteSpace(req.StudentName) || !string.IsNullOrWhiteSpace(req.StudentCode)
            ? "OCCUPIED"
            : "AVAILABLE"
    };
    var created = await repo.CreateAsync(bed);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapBedResponse(created));
}

[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] BedUpdateRequest req)
{
    var bed = new Bed
    {
        Id = id,
        BedNumber = req.BedNumber,
        Status = req.Status,
        StudentId = req.StudentId,
        StudentName = req.StudentName,
        StudentCode = req.StudentCode
    };
    var updated = await repo.UpdateAsync(bed);
    return Ok(MapBedResponse(updated));
}

[HttpPatch("{id}/status")]
public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] BedStatusUpdateRequest req)
{
    var existing = await repo.GetByIdAsync(id);
    if (existing is null) return NotFound();

    var updated = await repo.UpdateAsync(new Bed
    {
        Id = id,
        BedNumber = existing.BedNumber,
        Status = req.Status,
        StudentId = req.StudentId,
        StudentName = req.StudentName,
        StudentCode = req.StudentCode
    });

    return Ok(MapBedResponse(updated));
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)
{
    await repo.DeleteAsync(id);
    return NoContent();
}

private static BedResponse MapBedResponse(Bed bed) => new()
{
    Id = bed.Id,
    RoomId = bed.RoomId,
    BedNumber = bed.BedNumber,
    Status = bed.Status,
    StudentId = bed.StudentId,
    StudentName = bed.StudentName,
    StudentCode = bed.StudentCode,
    CreatedAt = bed.CreatedAt,
    UpdatedAt = bed.UpdatedAt
};
}
