using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MedCentre.models;
using MedCentre.service;

namespace MedCentre;

public partial class ProductManagementWindow : Window
{
    public ObservableCollection<Product> Products { get; set; }
    
    private static readonly ImageSource DefaultImage =
        new BitmapImage(new Uri("pack://application:,,,/GreenParadise;component/Resources/picture.png"));

    public ProductManagementWindow()
    {
        InitializeComponent();
        LoadProducts();
        DataContext = this;
    }

    private void LoadProducts()
    {
        var productService = new ProductService();
        Products = productService.LoadProducts();
        ProductsListView.ItemsSource = Products;
    }

    private void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        var productForm = new ProductFormWindow();
        if (productForm.ShowDialog() == true)
        {
            LoadProducts();
        }
    }

    private void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductsListView.SelectedItem is Product selectedProduct)
        {
            var productForm = new ProductFormWindow(selectedProduct);
            if (productForm.ShowDialog() == true)
            {
                LoadProducts();
            }
        }
    }
}