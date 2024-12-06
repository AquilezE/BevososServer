using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
