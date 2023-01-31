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
        return await _context.Categories
            .Include(c => c.Products)
            .Include(p=>p.Photo)
            .Where(c => c.Id == id)
            .ProjectTo<CategoryWithProductsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        // .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories.Include(p => p.Photo)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    } 
    public void UpdateCategory(Category updatedCat)
    {
       // _context.Categories.Update(updatedCat);
        _context.Entry(updatedCat).State = EntityState.Modified;
    }

    public void DeleteCategory(Category category)
    {
        _context.Categories.Remove(category);
    }
}