namespace RoomBuildingService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Api.DTOs.BuildingFacilityContract;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Interfaces;
[ApiController]
[Route("api/building-facility-contracts")]
public class BuildingFacilityContractsController(IBuildingFacilityContractRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? facilityId, [FromQuery] Guid? pricingId, [FromQuery] string? status)
        => Ok((await repo.GetAllAsync(facilityId, pricingId, status)).Select(MapResponse));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var contract = await repo.GetByIdAsync(id);
        if (contract is null) return NotFound();
        return Ok(MapResponse(contract));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BuildingFacilityContractCreateRequest req)
    {
        var created = await repo.CreateAsync(new BuildingFacilityContract
        {
            FacilityId = req.FacilityId,
            PricingId = req.PricingId,
            ContractCode = req.ContractCode,
            StudentId = req.StudentId,
            StudentName = req.StudentName,
            StudentCode = req.StudentCode,
            ContractType = req.ContractType,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            TotalAmount = req.TotalAmount,
            Notes = req.Notes
        });

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BuildingFacilityContractUpdateRequest req)
    {
        var updated = await repo.UpdateAsync(new BuildingFacilityContract
        {
            Id = id,
            StudentId = req.StudentId,
            StudentName = req.StudentName,
            StudentCode = req.StudentCode,
            ContractType = req.ContractType,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            TotalAmount = req.TotalAmount,
            Status = req.Status,
            Notes = req.Notes
        });

        return Ok(MapResponse(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await repo.DeleteAsync(id);
        return NoContent();
    }

    private static BuildingFacilityContractResponse MapResponse(BuildingFacilityContract contract) => new()
    {
        Id = contract.Id,
        FacilityId = contract.FacilityId,
        PricingId = contract.PricingId,
        ContractCode = contract.ContractCode,
        StudentId = contract.StudentId,
        StudentName = contract.StudentName,
        StudentCode = contract.StudentCode,
        ContractType = contract.ContractType,
        StartDate = contract.StartDate,
        EndDate = contract.EndDate,
        TotalAmount = contract.TotalAmount,
        Status = contract.Status,
        Notes = contract.Notes,
        CreatedAt = contract.CreatedAt,
        UpdatedAt = contract.UpdatedAt
    };
}
