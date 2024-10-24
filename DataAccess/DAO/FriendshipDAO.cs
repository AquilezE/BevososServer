using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class FriendshipDAO
    {
        public bool AddFriendship(int user1Id, int user2Id)
        {
            using (var context = new BevososContext())
            {
                try
                {
                    var user1 = context.Users.FirstOrDefault(u => u.UserId == user1Id);
                    var user2 = context.Users.FirstOrDefault(u => u.UserId == user2Id);

                    if (user1 == null || user2 == null)
                    {
                        return false; 
                    }

                    bool friendshipExists = context.Friendships.Any(f =>
                        (f.User1Id == user1Id && f.User2Id == user2Id) ||
                        (f.User1Id == user2Id && f.User2Id == user1Id));

                    if (friendshipExists)
                    {
                        return false; 
                    }

                    // Check if there is any friend request between these users
                    bool friendRequestExists = context.FriendRequests.Any(fr =>
                        (fr.RequesterId == user1Id && fr.RequesteeId == user2Id) ||
                        (fr.RequesterId == user2Id && fr.RequesteeId == user1Id));

                    if (friendRequestExists)
                    {
                        return false; 
                    }

                    // Check if blocked
                    bool blocked = context.BlockedList.Any(b =>
                        (b.BlockerId == user1Id && b.BlockeeId == user2Id) ||
                        (b.BlockerId == user2Id && b.BlockeeId == user1Id));

                    if (blocked)
                    {
                        return false; 
                    }

                 
                    var friendship = new Friendship
                    {
                        User1Id = user1Id,
                        User2Id = user2Id
                    };

                    context.Friendships.Add(friendship);
                    context.SaveChanges();

                    return true; 
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }


        public bool RemoveFriendship(int user1Id, int user2Id)
        {
            using (var context = new BevososContext())
            {
                try
                {
                    
                    var friendship = context.Friendships.FirstOrDefault(f =>
                        (f.User1Id == user1Id && f.User2Id == user2Id) ||
                        (f.User1Id == user2Id && f.User2Id == user1Id));

                    if (friendship != null)
                    {
                        
                        context.Friendships.Remove(friendship);
                        context.SaveChanges();

                        return true; 
                    }
                    else
                    {
                        return false; 
                    }
                }
                catch (Exception)
                {
                    return false; 
                }
            }
        }


        public List<User> GetFriendshipList(int userId)
        {
            using (var context = new BevososContext())
            {
                var friends = context.Friendships
                    .Where(f => f.User1Id == userId || f.User2Id == userId)
                    .Select(f => f.User1Id == userId ? f.User2 : f.User1)
                    .ToList();

                return friends;
            }
        }
    }
}
