namespace RoomBuildingService.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Api.DTOs.Building;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Exceptions;
using RoomBuildingService.Core.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class BuildingsController(IBuildingRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var buildings = await repo.GetAllAsync();
        return Ok(buildings.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var building = await repo.GetByIdAsync(id);
        if (building is null) return NotFound();

        return Ok(MapToResponse(building));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BuildingCreateRequest req)
    {
        var building = new Building
        {
            Name = req.Name,
            TotalFloors = req.TotalFloors,
            GenderType = NormalizeGenderType(req.GenderType),
            Description = req.Description,
        };

        var created = await repo.CreateAsync(building);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BuildingUpdateRequest req)
    {
        var building = new Building
        {
            Id = id,
            Name = req.Name,
            TotalFloors = req.TotalFloors,
            GenderType = NormalizeGenderType(req.GenderType),
            Status = NormalizeStatus(req.Status),
            Description = req.Description,
        };

        var updated = await repo.UpdateAsync(building);
        return Ok(MapToResponse(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await repo.DeleteAsync(id);
        return NoContent();
    }

    private static BuildingResponse MapToResponse(Building building) => new()
    {
        Id = building.Id,
        Name = building.Name,
        TotalFloors = building.TotalFloors,
        Floors = Enumerable.Range(1, building.TotalFloors).ToList(),
        GenderType = building.GenderType,
        Status = building.Status,
        Description = building.Description,
        CreatedAt = building.CreatedAt,
        UpdatedAt = building.UpdatedAt,
    };

    private static string NormalizeGenderType(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new BusinessRuleException("GenderType is required.");

        var key = input.Trim().ToUpperInvariant();
        return key switch
        {
            "MALE" or "NAM" => "MALE",
            "FEMALE" or "NU" => "FEMALE",
            "MIXED" or "HONHOP" or "HON_HOP" => "MIXED",
            _ => throw new BusinessRuleException("GenderType must be MALE/FEMALE/MIXED (or Nam/Nu/HonHop)."),
        };
    }

    private static string NormalizeStatus(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new BusinessRuleException("Status is required.");

        var key = input.Trim().ToUpperInvariant();
        return key switch
        {
            "ACTIVE" => "ACTIVE",
            "INACTIVE" => "INACTIVE",
            "UNDER_MAINTENANCE" => "UNDER_MAINTENANCE",
            _ => throw new BusinessRuleException("Status must be ACTIVE/INACTIVE/UNDER_MAINTENANCE."),
        };
    }
}
