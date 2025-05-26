using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedCentre.models;

public class Address
{
    [Column("id")]
    public int Id { get; set; }
        
    [StringLength(60)]
    [Column("street")]
    public string Street { get; set; }
        
    [StringLength(60)]
    [Column("city")]
    public string City { get; set; }
        
    [StringLength(12)]
    [Column("postal_code")]
    public string PostalCode { get; set; }
        
    [StringLength(60)]
    [Column("country")]
    public string Country { get; set; }
        
    public virtual ICollection<User> Users { get; set; }
    public virtual ICollection<Supplier> Suppliers { get; set; }
}