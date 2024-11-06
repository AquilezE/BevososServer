

namespace DataAccess.Models
{
    public class Friendship
    {
        public int Id { get; set; }
        public virtual User User1 { get; set; }
        public virtual User User2 { get; set; }

        public int User1Id { get; set; }

        public int User2Id{ get; set; }

    }
}
