namespace ChargingApp.DTOs;

public class CategoryUpdateDto
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public IFormFile? ImageFile { get; set; }
}