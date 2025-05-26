using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedCentre.models;

public class Category
{
    [Column("id")]
    public int Id { get; set; }
        
    [Required]
    [StringLength(50)]
    [Column("category_name")]
    public string CategoryName { get; set; }
    
    [Column("description")]
    public string Description { get; set; }
    
    [Column("popularity")]
    public float Popularity { get; set; }
        
    public virtual ICollection<Product> Products { get; set; }
}
