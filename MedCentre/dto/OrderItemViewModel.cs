using System.ComponentModel;
using System.Windows.Media;

namespace MedCentre.dto;

public class OrderItemViewModel : INotifyPropertyChanged
{
    private int _quantity;
        
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Brand { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
    public int Discount { get; set; }
    public ImageSource Image { get; set; }
        
    public int Quantity 
    { 
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }
        
    public decimal TotalPrice => Quantity * DiscountedPrice;
        
    public event PropertyChangedEventHandler PropertyChanged;
        
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}