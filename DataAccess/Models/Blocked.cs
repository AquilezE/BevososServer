

namespace DataAccess.Models
{
    public class Blocked
    {
        public int Id { get; set; }
        public virtual User Blocker { get; set; }
        public virtual User Blockee { get; set; }
        public int BlockerId { get; set; }
        public int BlockeeId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Blocked other = (Blocked)obj;

            return BlockerId == other.BlockerId && BlockeeId == other.BlockeeId;
        }
    }
}
