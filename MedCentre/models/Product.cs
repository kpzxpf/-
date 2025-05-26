using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MedCentre.models;

public class Product
{
    [Column("id")]
    public int Id { get; set; }
        
    [Required]
    [StringLength(100)]
    [Column("product_name")]
    public string ProductName { get; set; }
        
    [StringLength(60)]
    [Column("brand")]
    public string Brand { get; set; }
        
    [Column(name: "price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
    
    [Column("quantity")]
    public int Quantity { get; set; }
    
    [Column("image_data", TypeName = "varbinary(max)")]
    public byte[] ImageData { get; set; }
    
    [Column("category_id")]
    public int CategoryId { get; set; }
    
    [Column("supplier_id")]
    public int SupplierId { get; set; }
        
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; }
        
    [ForeignKey("SupplierId")]
    public virtual Supplier Supplier { get; set; }
        
    public virtual ICollection<OrderItem> OrderItems { get; set; }
    
    public ImageSource Image
    {
        get
        {
            if (ImageData == null || ImageData.Length == 0)
            {
                return DefaultImage;
            }

            try
            {
                var bmp = new BitmapImage();
                using (var ms = new MemoryStream(ImageData))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();
                }
                return bmp;
            }
            catch
            {
                return DefaultImage;
            }
        }
    }

    private static readonly ImageSource DefaultImage =
        new BitmapImage(new Uri("pack://application:,,,/MedCentre;component/Resources/picture.png"));
}