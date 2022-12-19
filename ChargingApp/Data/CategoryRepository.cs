using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class CategoryRepository : ICategoryRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public CategoryRepository(DataContext context,IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddCategory(Category category)
    {
        _context.Categories.Add(category);
        // return await SaveAllAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
         var category = await _context.Categories
             .Include(c=>c.Products)
             .Include(p=>p.Photo)
             .FirstOrDefaultAsync(x => x.Id == categoryId);
         return category;
    }

    public async Task<CategoryWithProductsDto?> GetCategoryByIdProjectedAsync(int id)
    {
        return await _context.Categories.Include(c => c.Products)
            .Include(p=>p.Photo)
            .Where(c => c.Id == id)
            .ProjectTo<CategoryWithProductsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        // .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public Task<Category?> GetCategoryById(int categoryId)
    {
        return null;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories.Include(p => p.Photo)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
    public void UpdateCategory(Category updatedCat)
    {
        _context.Entry(updatedCat).State = EntityState.Modified;
    }

    public async Task<bool> DeleteCategoryByIdAsync(int categoryId)
    {
        var category = await GetCategoryByIdAsync(categoryId);
        if (category is null) return false;
        _context.Categories.Remove(category);
        return await SaveAllAsync();
    }
    
    public async Task<Category?> GetCategoryByEnglishNameAsync(string categoryName)
    {
        return await _context.Categories
            .Include(x => x.Photo)
            .FirstOrDefaultAsync(x => x.EnglishName == categoryName);
    }

    public async Task<Category?> GetCategoryByArabicNameAsync(string categoryName)
    {
        return await _context.Categories.FirstOrDefaultAsync(x => x.ArabicName == categoryName);
    }
}