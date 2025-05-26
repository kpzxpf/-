using System.ComponentModel.DataAnnotations.Schema;

namespace MedCentre.models;

public class Order
{
    [Column("id")]
    public int Id { get; set; }
    [Column("date")]
    public DateTime Date { get; set; }
        
    [Column(name: "total_amount", TypeName = "decimal(9,2)")]
    public decimal TotalAmount { get; set; }
    
    [Column("status_id")]
    public int StatusId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
        
    [ForeignKey("StatusId")]
    public virtual OrderStatus Status { get; set; }
        
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
        
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}