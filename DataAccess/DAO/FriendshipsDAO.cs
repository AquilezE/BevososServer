using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class FriendshipsDAO
    { 
        public bool AddFriendship(int user1Id, int user2Id)
        {
            return true;
        }

        public bool RemoveFriendship(int user1Id, int user2Id)
        {
            return true;
        }

        public List<Friendship> GetFriendshipList(int userId)
        {
            return new List<Friendship>();
        }
    }
}
