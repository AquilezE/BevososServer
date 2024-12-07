using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Stats
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public int Wins { get; set; }

        public int MonstersCreated { get; set; }

        public int AnnihilatedBabies { get; set; }
    }
}
