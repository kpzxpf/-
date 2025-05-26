using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MedCentre.dto;
using MedCentre.models;
using MedCentre.service;

namespace MedCentre
{
    public partial class OrderManagementWindow : Window, INotifyPropertyChanged
    {
        private readonly OrderService _orderService = new OrderService();
        private readonly Dictionary<int, int> _originalOrderStatuses = new Dictionary<int, int>();
        private bool _isInitializing = true;

        public ObservableCollection<OrderManagementViewModel> Orders { get; } = new ObservableCollection<OrderManagementViewModel>();
        public ObservableCollection<OrderManagementViewModel> FilteredOrders { get; } = new ObservableCollection<OrderManagementViewModel>();
        public ObservableCollection<OrderStatus> OrderStatuses { get; } = new ObservableCollection<OrderStatus>();

        private OrderManagementViewModel _selectedOrder;
        public OrderManagementViewModel SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    HasSelectedOrder = _selectedOrder != null;
                    OnPropertyChanged();
                }
            }
        }

        private bool _hasSelectedOrder;
        public bool HasSelectedOrder
        {
            get => _hasSelectedOrder;
            set
            {
                _hasSelectedOrder = value;
                OnPropertyChanged();
            }
        }

        public OrderManagementWindow()
        {
            InitializeComponent();
            DataContext = this;
            SortComboBox.SelectedIndex = 0;
            Loaded += OrderManagementWindow_Loaded;
        }

        private async void OrderManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isInitializing = false;
                FilterOrders();
            }
        }

        private async Task InitializeAsync()
        {
            _isInitializing = true;
            var statuses = await _orderService.GetOrderStatusesAsync();
            OrderStatuses.Clear();
            foreach (var s in statuses)
            {
                OrderStatuses.Add(s);
            }

            if (OrderStatuses.Any())
            {
                await LoadOrdersAsync();
            }
            else
            {
                MessageBox.Show("Не удалось загрузить статусы заказов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadOrdersAsync()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            _originalOrderStatuses.Clear();
            Orders.Clear();

            foreach (var order in orders)
            {
                _originalOrderStatuses[order.Id] = order.StatusId;
                var vm = OrderManagementViewModel.FromOrder(order);
                vm.Status = OrderStatuses.FirstOrDefault(s => s.Id == order.StatusId);
                if (vm.Status == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Статус с Id {order.StatusId} не найден для заказа {order.Id}");
                }
                vm.RefreshAvailabilityStatus();
                Orders.Add(vm);
            }
        }

        private void FilterOrders()
        {
            if (_isInitializing) return;

            var list = SortComboBox.SelectedIndex switch
            {
                1 => Orders.OrderBy(o => o.TotalAmount),
                2 => Orders.OrderByDescending(o => o.TotalAmount),
                _ => Orders.OrderByDescending(o => o.Date)
            };

            FilteredOrders.Clear();
            foreach (var o in list)
            {
                FilteredOrders.Add(o);
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitializing)
            {
                FilterOrders();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _isInitializing = true;
            try
            {
                await LoadOrdersAsync();
            }
            finally
            {
                _isInitializing = false;
                FilterOrders();
            }
        }

        private async void OrderStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (sender is not ComboBox combo || combo.Tag is not int orderId) return;
            if (combo.SelectedItem is not OrderStatus status) return;

            if (_originalOrderStatuses.TryGetValue(orderId, out var originalId) && originalId == status.Id)
            {
                return;
            }

            _isInitializing = true;
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(orderId, status.Id);
                if (success)
                {
                    _originalOrderStatuses[orderId] = status.Id;
                    MessageBox.Show("Статус заказа успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadOrdersAsync();
                    FilterOrders();
                }
                else
                {
                    MessageBox.Show("Не удалось обновить статус заказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    RestoreOriginalStatus(combo, orderId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                RestoreOriginalStatus(combo, orderId);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void RestoreOriginalStatus(ComboBox combo, int orderId)
        {
            if (_originalOrderStatuses.TryGetValue(orderId, out var id))
            {
                combo.SelectedItem = OrderStatuses.FirstOrDefault(s => s.Id == id);
            }
        }

        private void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedOrder = OrdersListView.SelectedItem as OrderManagementViewModel;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}