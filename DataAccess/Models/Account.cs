using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountId { get; set; } // Primary key

        [EmailAddress]
        [Required]
        [Index(IsUnique = true)] // Enforce uniqueness at the database level
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // Navigation property to User (One-to-One)
        public virtual User User { get; set; }

        // Navigation property to Token (One-to-One)
        public virtual Token Token { get; set; }
    }
}
