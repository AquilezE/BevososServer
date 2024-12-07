using DataAccess.Exceptions;
using DataAccess.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


namespace DataAccess.DAO
{
    public class FriendRequestData
    {
        public int FriendRequestId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public int ProfilePictureId { get; set; }
    }
    public class FriendRequestDAO
    {
        public int SendFriendRequest(int requesterId, int requesteeId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    User requester = context.Users.FirstOrDefault(u => u.UserId == requesterId);
                    User requestee = context.Users.FirstOrDefault(u => u.UserId == requesteeId);

                    if (requester == null || requestee == null)
                    {
                        return 0;
                    }

                    bool requestExists = context.FriendRequests.Any(fr =>
                        (fr.RequesterId == requesterId && fr.RequesteeId == requesteeId) ||
                        (fr.RequesterId == requesteeId && fr.RequesteeId == requesterId));

                    if (requestExists)
                    {
                        return 0;
                    }

                    var friendRequest = new FriendRequest
                    {
                        RequesterId = requesterId,
                        RequesteeId = requesteeId
                    };

                    context.FriendRequests.Add(friendRequest);
                    context.SaveChanges();

                    return friendRequest.Id;
                }
            });
        }

        public bool AcceptFriendRequest(int requestId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    FriendRequest friendRequest = context.FriendRequests.FirstOrDefault(fr => fr.Id == requestId);
                    if (friendRequest == null)
                    {
                        return false;
                    }

                    context.FriendRequests.Remove(friendRequest);
                    context.SaveChanges();

                    return true;
                }
            });
        }

        public bool DeclineFriendRequest(int requestId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    FriendRequest friendRequest = context.FriendRequests.FirstOrDefault(fr => fr.Id == requestId);
                    if (friendRequest == null)
                    {
                        return false;
                    }

                    context.FriendRequests.Remove(friendRequest);
                    context.SaveChanges();

                    return true;
                }
            });
        }

        public List<FriendRequest> GetPendingFriendRequests(int userId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    List<FriendRequest> pendingRequests = context.FriendRequests
                        .Where(fr => fr.RequesteeId == userId)
                        .Include(fr => fr.Requester)
                        .ToList();

                    return pendingRequests;
                }
            });
        }

        public List<FriendRequestData> GetFriendRequestForUser(int currentUserId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    List<FriendRequestData> friendRequests = context.FriendRequests
                        .Where(fr => fr.RequesteeId == currentUserId)
                        .Select(fr => new FriendRequestData
                        {
                            FriendRequestId = fr.Id,
                            SenderId = fr.Requester.UserId,
                            SenderName = fr.Requester.Username,
                            ProfilePictureId = fr.Requester.ProfilePictureId
                        })
                        .ToList();

                    return friendRequests;
                }
            });
        }

        public bool FriendRequestExists(int requestId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    return context.FriendRequests.Any(fr => fr.Id == requestId);
                }
            });
        }
    }
}
