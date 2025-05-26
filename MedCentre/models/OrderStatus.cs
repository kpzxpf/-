using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedCentre.models;

public class OrderStatus
{
    [Column("id")]
    public int Id { get; set; }
        
    [Required]
    [StringLength(40)]
    [Column("status_name")]
    public string StatusName { get; set; }
        
    public virtual ICollection<Order> Orders { get; set; }
}