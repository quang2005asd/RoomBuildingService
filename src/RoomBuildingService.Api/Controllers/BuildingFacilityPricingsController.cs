namespace RoomBuildingService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Api.DTOs.BuildingFacilityPricing;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Interfaces;
[ApiController]
[Route("api/building-facility-pricings")]
public class BuildingFacilityPricingsController(IBuildingFacilityPricingRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? facilityId)
        => Ok((await repo.GetAllAsync(facilityId)).Select(MapResponse));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var pricing = await repo.GetByIdAsync(id);
        if (pricing is null) return NotFound();
        return Ok(MapResponse(pricing));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BuildingFacilityPricingCreateRequest req)
    {
        var created = await repo.CreateAsync(new BuildingFacilityPricing
        {
            FacilityId = req.FacilityId,
            IsPaid = req.IsPaid,
            Price = req.Price,
            BillingCycle = req.BillingCycle,
            Description = req.Description
        });

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BuildingFacilityPricingUpdateRequest req)
    {
        var updated = await repo.UpdateAsync(new BuildingFacilityPricing
        {
            Id = id,
            IsPaid = req.IsPaid,
            Price = req.Price,
            BillingCycle = req.BillingCycle,
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

    private static BuildingFacilityPricingResponse MapResponse(BuildingFacilityPricing pricing) => new()
    {
        Id = pricing.Id,
        FacilityId = pricing.FacilityId,
        IsPaid = pricing.IsPaid,
        Price = pricing.Price,
        BillingCycle = pricing.BillingCycle,
        Status = pricing.Status,
        Description = pricing.Description,
        CreatedAt = pricing.CreatedAt,
        UpdatedAt = pricing.UpdatedAt
    };
}
