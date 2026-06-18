namespace RoomBuildingService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Api.DTOs.Equipment;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Interfaces;
[ApiController]
[Route("api/[controller]")]
public class EquipmentsController(IEquipmentRepository repo) : ControllerBase
{
// GET /api/equipments?roomId=...
[HttpGet]
public async Task<IActionResult> GetByRoom([FromQuery] Guid roomId)
{
var equipments = await repo.GetByRoomAsync(roomId);
return Ok(equipments.Select(e => new EquipmentResponse
{
Id             = e.Id,
RoomId         = e.RoomId,
EquipmentName  = e.EquipmentName,
EquipmentIndex = e.EquipmentIndex,
Status         = e.Status,
CreatedAt      = e.CreatedAt,
UpdatedAt      = e.UpdatedAt
}));
}
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var e = await repo.GetByIdAsync(id);
    if (e is null) return NotFound();
    return Ok(new EquipmentResponse
    {
        Id             = e.Id,
        RoomId         = e.RoomId,
        EquipmentName  = e.EquipmentName,
        EquipmentIndex = e.EquipmentIndex,
        Status         = e.Status,
        CreatedAt      = e.CreatedAt,
        UpdatedAt      = e.UpdatedAt
    });
}

[HttpPost]
public async Task<IActionResult> Create([FromBody] EquipmentCreateRequest req)
{
    var equipment = new RoomEquipment
    {
        RoomId        = req.RoomId,
        EquipmentName = req.EquipmentName
    };
    var created = await repo.CreateAsync(equipment);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, new EquipmentResponse
    {
        Id             = created.Id,
        RoomId         = created.RoomId,
        EquipmentName  = created.EquipmentName,
        EquipmentIndex = created.EquipmentIndex,
        Status         = created.Status,
        CreatedAt      = created.CreatedAt,
        UpdatedAt      = created.UpdatedAt
    });
}

// PATCH /api/equipments/{id}/status  ← Nhóm Maintenance gọi endpoint này
[HttpPatch("{id}/status")]
public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] EquipmentStatusUpdateRequest req)
{
    await repo.UpdateStatusAsync(id, req.Status);
    return NoContent();
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)
{
    await repo.DeleteAsync(id);
    return NoContent();
}
}