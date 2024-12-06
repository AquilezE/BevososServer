using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace DataAccess.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [Index("IX_UserUsername", IsUnique = true)]
        [MaxLength(50)]
        public string Username { get; set; }

        public int ProfilePictureId { get; set; } = 1;

        public virtual Account Account { get; set; }

        public virtual Stats Stats { get; set; }
    }
}
