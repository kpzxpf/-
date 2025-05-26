using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
}