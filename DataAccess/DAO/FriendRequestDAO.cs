using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class FriendRequestDAO
    {
        public bool SendFriendRequest(int requesterId, string requesteeUsername)
        {
            using (var context = new BevososContext())
            {
                try
                {
                    var requester = context.Users.FirstOrDefault(u => u.UserId == requesterId);
                    if (requester == null)
                    {
                        return false; 
                    }

                    var requestee = context.Users.FirstOrDefault(u => u.Username == requesteeUsername);
                    if (requestee == null)
                    {
                        return false; 
                    }

                    // Check if a friend request exists 
                    bool requestExists = context.FriendRequests.Any(fr =>
                        (fr.RequesterId == requesterId && fr.RequesteeId == requestee.UserId) ||
                        (fr.RequesterId == requestee.UserId && fr.RequesteeId == requesterId));

                    if (requestExists)
                    {
                        return false;
                    }

                    // Create a friend request
                    var friendRequest = new FriendRequest
                    {
                        RequesterId = requesterId,
                        RequesteeId = requestee.UserId
                    };

                    context.FriendRequests.Add(friendRequest);
                    context.SaveChanges();

                    return true; 
                }
                catch (Exception)
                {
                    return false; 
                }
            }
        }


        public bool AcceptFriendRequest(int requestId)
        {
            using (var context = new BevososContext())
            {
                try
                {
                    // Find request
                    var friendRequest = context.FriendRequests.FirstOrDefault(fr => fr.Id == requestId);
                    if (friendRequest == null)
                    {
                        return false; 
                    }

                    
                    context.FriendRequests.Remove(friendRequest);
                    context.SaveChanges();

                    /*
                     * 
                     * HERE I'VE GOT NO CLUE IF WE SHOULD ADD THE FRIENDSHIP TO THE DATABASE OR NOT
                     * MAYBE HANDLE THIS IN THE SERVICE LAYER
                     * 
                     */

                    return true; 
                }
                catch (Exception)
                {
                    return false; 
                }
            }
        }

        public bool DeclineFriendRequest(int requestId)
        {
            using (var context = new BevososContext())
            {
                try
                {
                    var friendRequest = context.FriendRequests.FirstOrDefault(fr => fr.Id == requestId);
                    if (friendRequest == null)
                    {
                        return false; 
                    }

                    context.FriendRequests.Remove(friendRequest);
                    context.SaveChanges();

                    return true; 
                }
                catch (Exception)
                {
                    return false; 
                }
            }
        }


        //this stuff is taking way to long! 4 seconds is insanity
        public List<FriendRequest> GetPendingFriendRequests(int userId)
        {
            using (var context = new BevososContext())
            {
               
                var pendingRequests = context.FriendRequests
                    .Where(fr => fr.RequesteeId == userId)
                    .Include(fr => fr.Requester)
                    .ToList();

                return pendingRequests;
            }
        }

    }
}
