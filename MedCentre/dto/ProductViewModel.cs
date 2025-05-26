using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MedCentre.models;

public class ProductViewModel
{
    private static readonly ImageSource DefaultImage =
            new BitmapImage(new Uri("pack://application:,,,/MedCentre;component/Resources/picture.png"));
        
        private int _discount;
        private decimal _discountedPrice;
        private Brush _highlightBackground;

        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string CategoryName { get; set; }
        public ImageSource Image { get; }

        public int Discount 
        { 
            get => _discount;
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged(nameof(Discount));
                    OnPropertyChanged(nameof(HasDiscount));
                    OnPropertyChanged(nameof(HasNoDiscount));
                }
            }
        }

        public decimal DiscountedPrice
        {
            get => _discountedPrice;
            set
            {
                if (_discountedPrice != value)
                {
                    _discountedPrice = value;
                    OnPropertyChanged(nameof(DiscountedPrice));
                }
            }
        }

        public Brush HighlightBackground
        {
            get => _highlightBackground;
            set
            {
                if (_highlightBackground != value)
                {
                    _highlightBackground = value;
                    OnPropertyChanged(nameof(HighlightBackground));
                }
            }
        }

        public bool HasDiscount => Discount > 0;
        public bool HasNoDiscount => Discount == 0;

        public ProductViewModel(Product product)
        {
            Id = product.Id;
            ProductName = product.ProductName;
            Brand = product.Brand;
            Price = product.Price;
            Quantity = product.Quantity;
            CategoryName = product.Category?.CategoryName;
            Image = LoadImage(product.ImageData);
            
            _discount = 0;
            _discountedPrice = Price;
            _highlightBackground = new SolidColorBrush(Colors.White);
        }

        private ImageSource LoadImage(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return DefaultImage;
            }

            var bmp = new BitmapImage();
            using (var ms = new MemoryStream(data))
            {
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
            }
            return bmp;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
}