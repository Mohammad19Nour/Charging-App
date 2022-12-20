namespace ChargingApp.DTOs;

public class CategoryInfo
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public IFormFile? ImageFile { get; set; }
}
public class NewCategoryDto : CategoryInfo
{
    public bool HasSubCategories { get; set; } = true;
} 
public class CategoryUpdateDto : CategoryInfo
{
}