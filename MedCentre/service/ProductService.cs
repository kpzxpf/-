using System.Collections.ObjectModel;
using MedCentre.db;
using MedCentre.models;
using Microsoft.EntityFrameworkCore;

namespace MedCentre.service;

public class ProductService
{
    private ApplicationDbContext _context;

    public ProductService()
    {
        _context = new ApplicationDbContext();
    }
        
    public ObservableCollection<Product> LoadProducts()
    {
        List<Product> products = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToList();

        return new ObservableCollection<Product>(products);
    }
    
    public int GetTotalQuantityByPartner(int userId)
    {
        return _context.OrderItems
                   .Where(orderItem => orderItem.Order.UserId == userId)
                   .Sum(orderItem => (int?)orderItem.Quantity)
               ?? 0; 
    }

    public List<String> GetAllBrand()
    {
        return _context.Products.ToList().ConvertAll(product => product.Brand);
    }
}