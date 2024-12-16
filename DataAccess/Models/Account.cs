using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Account
    {
        [Key, ForeignKey("User")] 
        public int UserId { get; set; } 

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public virtual User User { get; set; }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            Account other = (Account)obj;

            return Email == other.Email && PasswordHash == other.PasswordHash;

        }
    }
}
