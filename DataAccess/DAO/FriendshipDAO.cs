using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;


namespace DataAccess.DAO
{
    public class FriendData
    {
        public int FriendshipId { get; set; }
        public int FriendId { get; set; }
        public string FriendName { get; set; }
        public int ProfilePictureId { get; set; }
        public bool IsConnected { get; set; }
    }
    public class FriendshipDAO
    {

        public Friendship AddFriendship(int user1Id, int user2Id)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    User user1 = context.Users.FirstOrDefault(u => u.UserId == user1Id);
                    User user2 = context.Users.FirstOrDefault(u => u.UserId == user2Id);

                    if (user1 == null || user2 == null)
                    {
                        return null;
                    }

                    bool friendshipExists = context.Friendships.Any(f =>
                        (f.User1Id == user1Id && f.User2Id == user2Id) ||
                        (f.User1Id == user2Id && f.User2Id == user1Id));

                    if (friendshipExists)
                    {
                        return null;
                    }

                    bool friendRequestExists = context.FriendRequests.Any(fr =>
                        (fr.RequesterId == user1Id && fr.RequesteeId == user2Id) ||
                        (fr.RequesterId == user2Id && fr.RequesteeId == user1Id));

                    if (friendRequestExists)
                    {
                        return null;
                    }

                    bool blocked = context.BlockedList.Any(b =>
                        (b.BlockerId == user1Id && b.BlockeeId == user2Id) ||
                        (b.BlockerId == user2Id && b.BlockeeId == user1Id));

                    if (blocked)
                    {
                        return null;
                    }

                    Friendship friendship = new Friendship
                    {
                        User1Id = user1Id,
                        User2Id = user2Id
                    };

                    context.Friendships.Add(friendship);
                    context.SaveChanges();

                    return friendship;
                }
            });
        }

        public bool FriendshipExists(int user1Id, int user2Id)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    return context.Friendships.Any(f =>
                        (f.User1Id == user1Id && f.User2Id == user2Id) ||
                        (f.User1Id == user2Id && f.User2Id == user1Id));
                }
            });
        }

        public bool RemoveFriendship(int user1Id, int user2Id)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    Friendship friendship = context.Friendships.FirstOrDefault(f =>
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
            });
        }

        public List<User> GetFriendshipList(int userId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    List<User> friends = context.Friendships
                        .Where(f => f.User1Id == userId || f.User2Id == userId)
                        .Select(f => f.User1Id == userId ? f.User2 : f.User1)
                        .ToList();

                    return friends;
                }
            });
        }

        public List<FriendData> GetFriendsForUser(int currentUserId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    List<FriendData> friends = context.Friendships
                        .Where(f => f.User1Id == currentUserId || f.User2Id == currentUserId)
                        .Select(f => new FriendData
                        {
                            FriendshipId = f.Id,
                            FriendId = f.User1Id == currentUserId ? f.User2.UserId : f.User1.UserId,
                            FriendName = f.User1Id == currentUserId ? f.User2.Username : f.User1.Username,
                            ProfilePictureId = f.User1Id == currentUserId ? f.User2.ProfilePictureId : f.User1.ProfilePictureId,
                            IsConnected = false
                        })
                        .ToList();

                    return friends;
                }
            });
        }

    }
}
