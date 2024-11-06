

namespace DataAccess.Models
{

    public class FriendRequest
    {
        public int Id { get; set; }
        public virtual User Requester { get; set; }
        public virtual User Requestee { get; set; }

        public int RequesterId { get; set; }
        public int RequesteeId { get; set; }


    }

}
