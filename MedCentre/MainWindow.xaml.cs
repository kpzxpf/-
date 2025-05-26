using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MedCentre.models;
using MedCentre.service;

namespace MedCentre
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ProductService _productService;
        private ObservableCollection<ProductViewModel> _products;
        private ObservableCollection<ProductViewModel> _filteredProducts;
        private CategoryService _categoryService;
        private UserSession _userSession;
        private OrderManager _orderManager;
        private const int MAX_SALES = 10;
        
        public ObservableCollection<ProductViewModel> Products
        {
            get => _products;
            set
            {
                if (_products != value)
                {
                    _products = value;
                    OnPropertyChanged(nameof(Products));
                    FilterProducts();
                }
            }
        }

        public ObservableCollection<ProductViewModel> FilteredProducts
        {
            get => _filteredProducts;
            set
            {
                if (_filteredProducts != value)
                {
                    _filteredProducts = value;
                    OnPropertyChanged(nameof(FilteredProducts));
                }
            }
        }

        public bool HasItems => _orderManager.HasItems;

        public MainWindow()
        {
            InitializeComponent();

            _categoryService = new CategoryService();
            _productService = new ProductService();
            _userSession = UserSession.Instance;
            _orderManager = OrderManager.Instance;
            
            _orderManager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(OrderManager.HasItems))
                {
                    OnPropertyChanged(nameof(HasItems));
                }
            };

            _categoryService.LoadCategoriesName()
                .ForEach(category => CategoryComboBox.Items.Add(category));
            
            _productService.GetAllBrand()
                .ForEach(brand =>BrandComboBox.Items.Add(brand));
                
            CategoryComboBox.SelectedIndex = 0;
            SortComboBox.SelectedIndex = 0;
            BrandComboBox.SelectedIndex = 0;
            
            if (_userSession.CurrentUser.Role.Id != 1)
            {
                ManagerOrderButton.Visibility = Visibility.Visible;
            }
            
            _filteredProducts = new ObservableCollection<ProductViewModel>();
            DataContext = this;

            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                var productsData = _productService.LoadProducts();

                _products = new ObservableCollection<ProductViewModel>(
                    productsData.Select(product =>
                    {
                        var productViewModel = new ProductViewModel(product);
                        productViewModel.Discount = CalculateDiscount(
                            _productService.GetTotalQuantityByPartner(_userSession.CurrentUser.Id));
                        productViewModel.DiscountedPrice =
                            productViewModel.Price * (1 - productViewModel.Discount / 100.0m);

                        if (productViewModel.Discount >= 15)
                        {
                            productViewModel.HighlightBackground =
                                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7fff00"));
                        }
                        else
                        {
                            productViewModel.HighlightBackground = new SolidColorBrush(Colors.White);
                        }

                        return productViewModel;
                    })
                );

                FilterProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CalculateDiscount(long totalQuantity)
        {
            if (totalQuantity < MAX_SALES / 4)
                return 0;
            else if (totalQuantity < MAX_SALES / 2)
                return 5;
            else if (totalQuantity < (3 * MAX_SALES) / 4)
                return 10;
            else
                return 15;
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterProducts();
        }

        private void DiscountFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterProducts();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterProducts();
        }
        
        private void BrandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterProducts();
        }

        private void FilterProducts()
        {
            if (_products == null)
                return;

            IEnumerable<ProductViewModel> filteredList = _products;
            
            if (CategoryComboBox.SelectedIndex != 0)
            {
                string? categoryName = CategoryComboBox.SelectedItem.ToString();

                filteredList = filteredList.Where(p=> p.CategoryName.Equals(categoryName));
            }

            if (BrandComboBox.SelectedIndex != 0)
            {
                string? brand = BrandComboBox.SelectedItem.ToString();
                
                filteredList = filteredList.Where(p => p.Brand.Equals(brand));
            }

            switch (SortComboBox?.SelectedIndex ?? 0)
            {
                case 1:
                    filteredList = filteredList.OrderBy(p => p.Price);
                    break;
                case 2:
                    filteredList = filteredList.OrderByDescending(p => p.Price);
                    break;
                default:
                    break;
            }

            FilteredProducts = new ObservableCollection<ProductViewModel>(filteredList);
        }

        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListView.SelectedItem is ProductViewModel selectedProduct)
            {
                _orderManager.AddProduct(selectedProduct);
                MessageBox.Show($"Товар '{selectedProduct.ProductName}' добавлен к заказу",
                    "Товар добавлен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите товар для добавления к заказу",
                    "Товар не выбран", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderWindow();
            orderWindow.Owner = this;
            orderWindow.ShowDialog();
        }
        
        private void ManagerOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var managerOrderWindow = new OrderManagementWindow();
            managerOrderWindow.Owner = this;
            managerOrderWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}