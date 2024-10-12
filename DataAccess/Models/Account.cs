using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountId { get; set; } // Primary Key

        [EmailAddress]
        [Required]
        [Index("IX_AccountEmail", IsUnique = true)] // Unique constraint on Email
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Store hashed password

        // Foreign Key to User
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Navigation property to Token (if needed)
        public virtual Token Token { get; set; }
    }
}
