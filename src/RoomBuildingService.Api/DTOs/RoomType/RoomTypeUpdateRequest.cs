namespace RoomBuildingService.Api.DTOs.RoomType;
using System.ComponentModel.DataAnnotations;
public class RoomTypeUpdateRequest
{
    [Required] [MaxLength(50)]  public string   TypeName    { get; set; } = null!;
    [Required] [Range(1, 20)]   public int      Capacity    { get; set; }
    [Required] [Range(typeof(decimal), "0", "79228162514264337593543950335")] public decimal BasePrice { get; set; }
    public string?  ImageUrl    { get; set; }
    public string?  Description { get; set; }
    public List<string> Amenities { get; set; } = new();
}
