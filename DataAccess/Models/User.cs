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
        public int UserId { get; set; }
        [Required]
        [Index(IsUnique = true)] // Enforces unique Username
        [MaxLength(50)]
        public string Username { get; set; }

        public int ProfilePictureId { get; set; } = 1;

        public Account Account { get; set; }
    }
}
