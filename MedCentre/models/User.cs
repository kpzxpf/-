using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Controls;

namespace MedCentre.models;

public class User
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; }
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; }
        
        [StringLength(11)]
        [Column("phone")]
        public string Phone { get; set; }
        
        [Required]
        [StringLength(150)]
        [Column("login")]
        public string Login { get; set; }
        
        [Required]
        [StringLength(90)]
        [Column("password")]
        public string Password { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [Column("role_id")]
        public int RoleId { get; set; }
        
        [Column("address_id")]
        public int AddressId { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        
        [ForeignKey("AddressId")]
        public virtual Address Address { get; set; }
        
        public virtual ICollection<Order> Orders { get; set; }
        
        public User(String name, String login, String password, String email)
        {
            this.Name = name;
            this.Login = login;
            this.Password = password;
            this.Email = email;
        }
    }
