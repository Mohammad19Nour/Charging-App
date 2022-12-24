using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingApp.Entity;

[Table("Photos")]
public class Photo :BaseEntity
{
    public string? Url { get; set; }
    public string? PublicId { get; set; }
}