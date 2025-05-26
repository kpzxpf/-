using System.Collections.ObjectModel;
using System.ComponentModel;
using MedCentre.dto;

namespace MedCentre.models;

public sealed class OrderManager : INotifyPropertyChanged
{
    private static readonly Lazy<OrderManager> _instance = 
        new Lazy<OrderManager>(() => new OrderManager());
        
    public static OrderManager Instance => _instance.Value;
    
    private ObservableCollection<OrderItemViewModel> _currentOrderItems;
    private bool _hasItems;
    
    private OrderManager() 
    {
        _currentOrderItems = new ObservableCollection<OrderItemViewModel>();
        _currentOrderItems.CollectionChanged += (sender, e) => 
        {
            HasItems = _currentOrderItems.Count > 0;
        };
    }
    
    public ObservableCollection<OrderItemViewModel> CurrentOrderItems => _currentOrderItems;
    
    public bool HasItems
    {
        get => _hasItems;
        private set
        {
            if (_hasItems != value)
            {
                _hasItems = value;
                OnPropertyChanged(nameof(HasItems));
            }
        }
    }
    
    public void AddProduct(ProductViewModel product)
    {
        var existingItem = _currentOrderItems.FirstOrDefault(item => item.ProductId == product.Id);
        
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            var orderItem = new OrderItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Brand = product.Brand,
                Price = product.Price,
                DiscountedPrice = product.DiscountedPrice,
                Discount = product.Discount,
                Image = product.Image,
                Quantity = 1
            };
            
            _currentOrderItems.Add(orderItem);
        }
    }
    
    public void RemoveProduct(int productId)
    {
        var item = _currentOrderItems.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _currentOrderItems.Remove(item);
        }
    }
    
    public void ClearOrder()
    {
        _currentOrderItems.Clear();
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}