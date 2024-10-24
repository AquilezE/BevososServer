using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class FriendRequestDAO
    {
        public bool AddFriendRequest(int requesterId, int requesteeId)
        {
            return true;
        }

        public bool DeleteFriendRequest(int requesterId, int requesteeId)
        {
            return true;
        }

        public List<FriendRequest> GetFriendRequestList(int userId)
        {
            return new List<FriendRequest>();
        }
    }
}
