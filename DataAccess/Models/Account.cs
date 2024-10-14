using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; } // Primary Key

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(100)]
        public string Email { get; set; } // Email associated with the account

        [Required]
        public string PasswordHash { get; set; } // Hashed password

        public int UserId { get; set; } // Foreign key for User

        public virtual User User { get; set; } // Navigation property to the User
    }
}
