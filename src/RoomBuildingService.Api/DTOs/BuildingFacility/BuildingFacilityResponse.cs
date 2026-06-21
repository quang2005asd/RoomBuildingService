namespace RoomBuildingService.Api.DTOs.BuildingFacility;
public class BuildingFacilityResponse
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string FacilityName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
