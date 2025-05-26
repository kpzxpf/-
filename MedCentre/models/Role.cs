using System.ComponentModel.DataAnnotations.Schema;

namespace MedCentre.models;

public class Role
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("role_name")]
    public string RoleName { get; set; }
        
    public virtual ICollection<User> Users { get; set; }
}