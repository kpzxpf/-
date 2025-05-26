using MedCentre.db;

namespace MedCentre.service;

public class CategoryService
{
    private ApplicationDbContext _context;

    public CategoryService()
    {
        _context = new ApplicationDbContext();
    }
        
    public List<String> LoadCategoriesName()
    {
        return _context.Categories.ToList().ConvertAll(input => input.CategoryName);
    }
}