using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using MedCentre.models;
using MedCentre.service;

namespace MedCentre.dto
{
    public class OrderManagementViewModel : INotifyPropertyChanged
    {
        private int _id;
        private DateTime _date;
        private decimal _totalAmount;
        private OrderStatus _status;
        private string _customerName;
        private string _customerDetails;
        private List<OrderItem> _orderItems;
        private Brush _rowBackground;
        private string _availabilityStatus;
        private Brush _availabilityColor;
        private ProductService _productService;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public OrderStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (_customerName != value)
                {
                    _customerName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CustomerDetails
        {
            get => _customerDetails;
            set
            {
                if (_customerDetails != value)
                {
                    _customerDetails = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<OrderItem> OrderItems
        {
            get => _orderItems;
            set
            {
                if (_orderItems != value)
                {
                    _orderItems = value;
                    OnPropertyChanged();
                    UpdateAvailabilityStatus();
                }
            }
        }

        public Brush RowBackground
        {
            get => _rowBackground;
            set
            {
                if (_rowBackground != value)
                {
                    _rowBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AvailabilityStatus
        {
            get => _availabilityStatus;
            set
            {
                if (_availabilityStatus != value)
                {
                    _availabilityStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush AvailabilityColor
        {
            get => _availabilityColor;
            set
            {
                if (_availabilityColor != value)
                {
                    _availabilityColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public OrderManagementViewModel()
        {
            _productService = new ProductService();
            _availabilityStatus = "Загрузка...";
            _availabilityColor = new SolidColorBrush(Colors.Gray);
            _rowBackground = new SolidColorBrush(Colors.White);
        }

        public static OrderManagementViewModel FromOrder(Order order)
        {
            var viewModel = new OrderManagementViewModel
            {
                Id = order.Id,
                Date = order.Date,
                TotalAmount = order.TotalAmount,
                Status = null, // Status устанавливается в LoadOrdersAsync
                CustomerName = $"{order.User.Name}",
                CustomerDetails = $"Клиент: {order.User.Name}\n" +
                                 $"Email: {order.User.Email ?? "Не указан"}\n" +
                                 $"Телефон: {order.User.Phone ?? "Не указан"}",
                OrderItems = order.OrderItems?.ToList() ?? new List<OrderItem>()
            };

            return viewModel;
        }

        private void UpdateAvailabilityStatus()
        {
            try
            {
                if (OrderItems == null || !OrderItems.Any())
                {
                    SetAvailabilityStatus("Нет товаров", Colors.Gray, Colors.White);
                    return;
                }

                if (_productService == null)
                {
                    _productService = new ProductService();
                }

                var products = _productService.LoadProducts();
                if (products == null || !products.Any())
                {
                    SetAvailabilityStatus("Ошибка загрузки", Colors.Red, Colors.White);
                    return;
                }

                bool allAvailable = true;

                foreach (var item in OrderItems)
                {
                    if (item.Product == null)
                    {
                        allAvailable = false;
                        break;
                    }

                    var currentProduct = products.FirstOrDefault(p => p.Id == item.Product.Id);
                    if (currentProduct == null || currentProduct.Quantity < item.Quantity)
                    {
                        allAvailable = false;
                        break;
                    }
                }

                if (allAvailable)
                {
                    SetAvailabilityStatus("В наличии",
                        (Color)ColorConverter.ConvertFromString("#20b2aa"),
                        (Color)ColorConverter.ConvertFromString("#20b2aa"),
                        0.1);
                }
                else
                {
                    SetAvailabilityStatus("Не в наличии",
                        (Color)ColorConverter.ConvertFromString("#ff8c00"),
                        (Color)ColorConverter.ConvertFromString("#ff8c00"),
                        0.1);
                }
            }
            catch (Exception ex)
            {
                SetAvailabilityStatus("Ошибка проверки", Colors.Red, Colors.White);
                System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении статуса наличия: {ex.Message}");
            }
        }

        private void SetAvailabilityStatus(string status, Color textColor, Color backgroundColor, double backgroundOpacity = 1.0)
        {
            AvailabilityStatus = status;
            AvailabilityColor = new SolidColorBrush(textColor);
            RowBackground = new SolidColorBrush(backgroundColor) { Opacity = backgroundOpacity };
        }

        public void RefreshAvailabilityStatus()
        {
            UpdateAvailabilityStatus();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}