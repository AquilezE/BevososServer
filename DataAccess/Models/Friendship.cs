namespace DataAccess.Models
{
    public class Friendship
    {
        public int Id { get; set; }
        public virtual User User1 { get; set; }
        public virtual User User2 { get; set; }

        public int User1Id { get; set; }

        public int User2Id { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var other = (Friendship)obj;

            return User1Id == other.User1Id && User2Id == other.User2Id;
        }
    }
}