using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MedCentre.db;
using MedCentre.models;
using Microsoft.Win32;

namespace MedCentre
{
    public partial class ProductFormWindow : Window
    {
        private Product _product;
        private byte[] _imageData;
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ProductFormWindow(Product product = null)
        {
            InitializeComponent();
            LoadCategories();
            LoadSuppliers();

            _product = product ?? new Product();
            if (product != null)
            {
                _product = new Product
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    CategoryId = product.CategoryId,
                    SupplierId = product.SupplierId,
                    Brand = product.Brand,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    ImageData = product.ImageData
                };
                PopulateFields();
                DeleteButton.Visibility = Visibility.Visible;
            }
            else
            {
                _product = new Product();
                DeleteButton.Visibility = Visibility.Collapsed;
            }

        }

        private void LoadCategories()
        {
            CategoryComboBox.ItemsSource = _context.Categories.ToList();
        }

        private void LoadSuppliers()
        {
            SupplierComboBox.ItemsSource = _context.Suppliers.ToList();
        }

        private void PopulateFields()
        {
            ProductNameTextBox.Text = _product.ProductName;
            CategoryComboBox.SelectedValue = _product.CategoryId;
            SupplierComboBox.SelectedValue = _product.SupplierId;
            BrandTextBox.Text = _product.Brand;
            PriceTextBox.Text = _product.Price.ToString("F2");
            QuantityTextBox.Text = _product.Quantity.ToString();
            if (_product.ImageData != null)
            {
                _imageData = _product.ImageData;
                ProductImage.Source = LoadImage(_imageData);
            }
        }

        private BitmapImage LoadImage(byte[] data)
        {
            var bmp = new BitmapImage();
            using (var ms = new MemoryStream(data))
            {
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            return bmp;
        }

        private void IntegerValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "." && !((TextBox)sender).Text.Contains("."))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = !decimal.TryParse(e.Text, out _);
            }
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _imageData = File.ReadAllBytes(openFileDialog.FileName);
                ProductImage.Source = LoadImage(_imageData);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductNameTextBox.Text))
            {
                MessageBox.Show("Введите наименование товара");
                return;
            }

            if (CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию");
                return;
            }

            if (SupplierComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите поставщика");
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть неотрицательным числом");
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным целым числом");
                return;
            }

            _product.ProductName = ProductNameTextBox.Text;
            _product.CategoryId = (int)CategoryComboBox.SelectedValue;
            _product.SupplierId = (int)SupplierComboBox.SelectedValue;
            _product.Brand = BrandTextBox.Text;
            _product.Price = price;
            _product.Quantity = quantity;
            _product.ImageData = _imageData;

            if (_product.Id == 0) 
            {
                _context.Products.Add(_product);
            }
            else 
            {
                _context.Products.Update(_product);
            }

            try
            {
                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_product.Id == 0)
            {
                MessageBox.Show("Невозможно удалить несохраненный товар");
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить товар \"{_product.ProductName}" +
                $"\"?\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)  
            {
                try
                {
                    _context.Products.Remove(_product);
                    _context.SaveChanges();

                    MessageBox.Show("Товар успешно удален", "Удаление",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении товара: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}