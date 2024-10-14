using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Account
    {
        [Key, ForeignKey("User")]  // UserId is both Primary Key and Foreign Key
        public int UserId { get; set; } // Foreign key and Primary key for User

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public virtual User User { get; set; } // Navigation property to the User
    }
}
