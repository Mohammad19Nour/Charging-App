using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingApp.Entity;
public class Photo :BaseEntity
{
    public string? Url { get; set; }
}