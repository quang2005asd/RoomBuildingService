namespace RoomBuildingService.Api.DTOs.RoomType;
public class RoomTypeResponse
{
    public Guid          Id          { get; set; }
    public string        TypeName    { get; set; } = null!;
    public int           Capacity    { get; set; }
    public decimal       BasePrice   { get; set; }
    public string?       ImageUrl    { get; set; }
    public string?       Description { get; set; }
    public List<string>  Amenities   { get; set; } = new();
    public DateTime      CreatedAt   { get; set; }
    public DateTime?     UpdatedAt   { get; set; }
}
