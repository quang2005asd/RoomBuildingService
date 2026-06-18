namespace RoomBuildingService.Api.DTOs.Building;
public class BuildingResponse
{
    public Guid      Id          { get; set; }
    public string    Name        { get; set; } = null!;
    public int       TotalFloors { get; set; }
    public List<int> Floors      { get; set; } = new();
    public string    GenderType  { get; set; } = null!;
    public string    Status      { get; set; } = null!;
    public string?   Description { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }
}
