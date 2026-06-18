namespace RoomBuildingService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Api.DTOs.RoomType;
using RoomBuildingService.Core.Entities;
using RoomBuildingService.Core.Interfaces;
[ApiController]
[Route("api/[controller]")]
public class RoomTypesController(IRoomTypeRepository repo) : ControllerBase
{
[HttpGet]
public async Task<IActionResult> GetAll()
{
var types = await repo.GetAllAsync();
var result = types.Select(rt => new RoomTypeResponse
{
Id          = rt.Id,
TypeName    = rt.TypeName,
Capacity    = rt.Capacity,
BasePrice   = rt.BasePrice,
ImageUrl    = rt.ImageUrl,
Description = rt.Description,
Amenities   = rt.Amenities.Select(a => a.AmenityName).ToList(),
CreatedAt   = rt.CreatedAt,
UpdatedAt   = rt.UpdatedAt
});
return Ok(result);
}
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var rt = await repo.GetByIdAsync(id);
    if (rt is null) return NotFound();
    return Ok(new RoomTypeResponse
    {
        Id          = rt.Id,
        TypeName    = rt.TypeName,
        Capacity    = rt.Capacity,
        BasePrice   = rt.BasePrice,
        ImageUrl    = rt.ImageUrl,
        Description = rt.Description,
        Amenities   = rt.Amenities.Select(a => a.AmenityName).ToList(),
        CreatedAt   = rt.CreatedAt,
        UpdatedAt   = rt.UpdatedAt
    });
}

[HttpPost]
public async Task<IActionResult> Create([FromBody] RoomTypeCreateRequest req)
{
    var roomType = new RoomType
    {
        TypeName    = req.TypeName,
        Capacity    = req.Capacity,
        BasePrice   = req.BasePrice,
        ImageUrl    = req.ImageUrl,
        Description = req.Description,
        Amenities   = req.Amenities
                         .Select(a => new RoomAmenity { AmenityName = a })
                         .ToList()
    };
    var created = await repo.CreateAsync(roomType);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, new RoomTypeResponse
    {
        Id          = created.Id,
        TypeName    = created.TypeName,
        Capacity    = created.Capacity,
        BasePrice   = created.BasePrice,
        ImageUrl    = created.ImageUrl,
        Description = created.Description,
        Amenities   = created.Amenities.Select(a => a.AmenityName).ToList(),
        CreatedAt   = created.CreatedAt,
        UpdatedAt   = created.UpdatedAt
    });
}

[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] RoomTypeUpdateRequest req)
{
    var roomType = new RoomType
    {
        Id          = id,
        TypeName    = req.TypeName,
        Capacity    = req.Capacity,
        BasePrice   = req.BasePrice,
        ImageUrl    = req.ImageUrl,
        Description = req.Description,
        Amenities   = req.Amenities
                         .Select(a => new RoomAmenity { AmenityName = a })
                         .ToList()
    };
    var updated = await repo.UpdateAsync(roomType);
    return Ok(new RoomTypeResponse
    {
        Id          = updated.Id,
        TypeName    = updated.TypeName,
        Capacity    = updated.Capacity,
        BasePrice   = updated.BasePrice,
        ImageUrl    = updated.ImageUrl,
        Description = updated.Description,
        Amenities   = updated.Amenities.Select(a => a.AmenityName).ToList(),
        CreatedAt   = updated.CreatedAt,
        UpdatedAt   = updated.UpdatedAt
    });
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)
{
    await repo.DeleteAsync(id);
    return NoContent();
}
}
