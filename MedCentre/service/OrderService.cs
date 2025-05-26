using MedCentre.db;
using MedCentre.dto;
using MedCentre.models;
using Microsoft.EntityFrameworkCore;

namespace MedCentre.service
{
    public class OrderService
    {
        private ApplicationDbContext _context;

        public OrderService()
        {
            _context = new ApplicationDbContext();
        }
        
        public async Task<Order> CreateOrderAsync(int userId, List<OrderItemViewModel> orderItems)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    Date = DateTime.Now,
                    StatusId = 1,
                    TotalAmount = 0,
                    OrderItems = new List<OrderItem>()
                };
                
                decimal totalAmount = 0;
                foreach (var item in orderItems.Where(i => i.Quantity > 0))
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice
                    };
                    
                    order.OrderItems.Add(orderItem);
                    totalAmount += item.TotalPrice;
                }
                
                order.TotalAmount = totalAmount;
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public int GetNextOrderNumber()
        {
            var lastOrder = _context.Orders.OrderByDescending(o => o.Id).FirstOrDefault();
            return (lastOrder?.Id ?? 0) + 1;
        }
        
        public int CalculateDeliveryDays(List<OrderItemViewModel> orderItems)
        {
            try
            {
                var availableCount = 0;
                foreach (var item in orderItems)
                {
                    var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null && product.Quantity >= 3)
                    {
                        availableCount++;
                    }
                }
                
                return availableCount >= 3 ? 3 : 6;
            }
            catch
            {
                return 6;
            }
        }
        
        public string GeneratePickupCode()
        {
            var random = new Random();
            return random.Next(100, 1000).ToString();
        }
        
        public async Task<List<Order>> GetOrdersByUserAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Status)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Date)
                .ToListAsync();
        }
        
        public async Task<bool> UpdateOrderStatusAsync(int orderId, int statusId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.StatusId = statusId; 
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Include(o => o.Status)
                .Include(o => o.User)
                .OrderByDescending(o => o.Date)
                .ToListAsync();
        }
        
        public async Task<List<OrderStatus>> GetOrderStatusesAsync()
        {
            return await _context.Set<OrderStatus>().ToListAsync();
        }
        
        public async Task<List<Order>> GetOrdersByStatusAsync(int statusId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Status)
                .Include(o => o.User)
                .Where(o => o.StatusId == statusId)
                .OrderByDescending(o => o.Date)
                .ToListAsync();
        }
        
        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Status)
                .Include(o => o.User)
                .Where(o => o.Date.Date >= startDate && o.Date.Date <= endDate)
                .OrderByDescending(o => o.Date)
                .ToListAsync();
        }
    }
}