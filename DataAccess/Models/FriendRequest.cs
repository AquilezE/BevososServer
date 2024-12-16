

namespace DataAccess.Models
{

    public class FriendRequest
    {
        public int Id { get; set; }
        public virtual User Requester { get; set; }
        public virtual User Requestee { get; set; }

        public int RequesterId { get; set; }
        public int RequesteeId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            FriendRequest other = (FriendRequest)obj;

            return RequesterId == other.RequesterId && RequesteeId == other.RequesteeId;
        }
    }

}
