namespace ChargingApp.DTOs;

public class NewCategoryDto
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public bool HasSubCategories { get; set; } = true;
    public IFormFile PhotoFile { get; set; }
}