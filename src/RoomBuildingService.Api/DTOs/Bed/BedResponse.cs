namespace RoomBuildingService.Api.DTOs.Bed;
public class BedResponse
{
    public Guid      Id          { get; set; }
    public Guid      RoomId      { get; set; }
    public string    BedNumber   { get; set; } = null!;
    public string    Status      { get; set; } = null!;
    public Guid?     StudentId   { get; set; }
    public string?   StudentName { get; set; }
    public string?   StudentCode { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }
}
