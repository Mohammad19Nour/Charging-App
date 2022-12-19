using System.ComponentModel.DataAnnotations;
using EntityFrameworkCore.EncryptColumn.Attribute;

namespace ChargingApp.Entity;

public class Category
{
    public int Id { get; set; }
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public Photo? Photo { get; set; }
    public ICollection<Product>? Products { get; set; }
    public bool HasSubCategories { get; set; }
    public bool Available { get; set; } = true;
}