using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedCentre.models;

public class Supplier
{
    [Column("id")]
    public int Id { get; set; }
        
    [Required]
    [StringLength(50)]
    [Column("name")]
    public string Name { get; set; }
        
    [StringLength(50)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; }
        
    [StringLength(11)]
    [Column("phone")]
    public string Phone { get; set; }
    
    [Column("address_id")]
    public int AddressId { get; set; }
        
    [ForeignKey("AddressId")]
    public virtual Address Address { get; set; }
        
    public virtual ICollection<Product> Products { get; set; }
}