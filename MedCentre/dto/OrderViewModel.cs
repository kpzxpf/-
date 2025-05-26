using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MedCentre.dto;

public class OrderViewModel : INotifyPropertyChanged
{
    private ObservableCollection<OrderItemViewModel> _orderItems;
    
    public int OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public string PickupPoint { get; set; }
    public string PickupCode { get; set; }
    public int DeliveryDays { get; set; }
    
    public ObservableCollection<OrderItemViewModel> OrderItems
    {
        get => _orderItems;
        set
        {
            if (_orderItems != value)
            {
                if (_orderItems != null)
                {
                    foreach (var item in _orderItems)
                    {
                        item.PropertyChanged -= OnOrderItemPropertyChanged;
                    }
                }
                
                _orderItems = value;
                
                if (_orderItems != null)
                {
                    foreach (var item in _orderItems)
                    {
                        item.PropertyChanged += OnOrderItemPropertyChanged;
                    }
                }
                
                OnPropertyChanged(nameof(OrderItems));
                UpdateTotals();
            }
        }
    }
    
    public decimal TotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal FinalAmount { get; private set; }
    
    private void OnOrderItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OrderItemViewModel.Quantity) || 
            e.PropertyName == nameof(OrderItemViewModel.TotalPrice))
        {
            UpdateTotals();
        }
    }
    
    private void UpdateTotals()
    {
        if (OrderItems == null) return;
        
        TotalAmount = OrderItems.Sum(item => item.Quantity * item.Price);
        var discountedTotal = OrderItems.Sum(item => item.TotalPrice);
        DiscountAmount = TotalAmount - discountedTotal;
        FinalAmount = discountedTotal;
        
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(FinalAmount));
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}