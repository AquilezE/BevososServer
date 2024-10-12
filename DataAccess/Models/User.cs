using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; } // Primary Key

        [Required]
        public string Username { get; set; }

        [EmailAddress]
        [Required]
        [Index("IX_UserEmail", IsUnique = true)] // Unique constraint on Email
        public string Email { get; set; }

        public int ProfilePictureId { get; set; } // Used in the client

        // Navigation properties
        public virtual Account Account { get; set; }
    }
}
