using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Blocked
    {
        public int Id { get; set; }
        public virtual User Blocker { get; set; }
        public virtual User Blockee { get; set; }
        public int BlockerId { get; set; }
        public int BlockeeId { get; set; }
    }
}
