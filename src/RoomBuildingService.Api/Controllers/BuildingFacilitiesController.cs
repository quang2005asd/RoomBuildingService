namespace RoomBuildingService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Api.DTOs.BuildingFacility;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Interfaces;
[ApiController]
[Route("api/building-facilities")]
public class BuildingFacilitiesController(IBuildingFacilityRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? buildingId)
        => Ok((await repo.GetAllAsync(buildingId)).Select(MapResponse));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var facility = await repo.GetByIdAsync(id);
        if (facility is null) return NotFound();
        return Ok(MapResponse(facility));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BuildingFacilityCreateRequest req)
    {
        var created = await repo.CreateAsync(new BuildingFacility
        {
            BuildingId = req.BuildingId,
            FacilityName = req.FacilityName,
            Description = req.Description
        });

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BuildingFacilityUpdateRequest req)
    {
        var updated = await repo.UpdateAsync(new BuildingFacility
        {
            Id = id,
            FacilityName = req.FacilityName,
            Status = req.Status,
            Description = req.Description
        });

        return Ok(MapResponse(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await repo.DeleteAsync(id);
        return NoContent();
    }

    private static BuildingFacilityResponse MapResponse(BuildingFacility facility) => new()
    {
        Id = facility.Id,
        BuildingId = facility.BuildingId,
        FacilityName = facility.FacilityName,
        Status = facility.Status,
        Description = facility.Description,
        CreatedAt = facility.CreatedAt,
        UpdatedAt = facility.UpdatedAt
    };
}
