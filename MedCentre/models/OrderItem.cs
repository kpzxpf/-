using System.ComponentModel.DataAnnotations.Schema;

namespace MedCentre.models;

public class OrderItem
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("quantity")]
    public int Quantity { get; set; }
        
    [Column(name: "total_price", TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }
    
    [Column("order_id")]
    public int OrderId { get; set; }
    
    [Column("product_id")]
    public int ProductId { get; set; }
        
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; }
        
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }
}
