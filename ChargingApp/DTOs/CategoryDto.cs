namespace ChargingApp.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public string? Photo { get; set; }
    public bool HasSubCategories { get; set; }
}

public class CategoryWithProductsDto : CategoryDto
{
    public List<ProductDto>? Products { get; set; }
}