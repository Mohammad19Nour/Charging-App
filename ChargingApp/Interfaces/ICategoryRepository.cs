using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface ICategoryRepository
{
    public Task<Category?> GetCategoryByEnglishNameAsync(string categoryName);
    public Task<Category?> GetCategoryByArabicNameAsync(string categoryName);
    public void AddCategory(Category category);
    public Task<Category?> GetCategoryByIdAsync(int categoryId);
    public Task<CategoryWithProductsDto?> GetCategoryByIdProjectedAsync(int id);
    public Task<Category?> GetCategoryById(int categoryId);
    public Task<List<CategoryDto>> GetAllCategoriesAsync();
    
    public void UpdateCategory(Category updatedCat);
    public void DeleteCategory(Category category);
}