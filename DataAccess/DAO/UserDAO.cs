using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;

namespace DataAccess.DAO
{
    public class UserDAO
    {
        public bool UsernameExists(string username)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    return context.Users.Any(u => u.Username == username);
                }
            });
        }

        public User GetUserByEmail(string email)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    var account = context.Accounts
                                         .Include("User")
                                         .FirstOrDefault(a => a.Email == email);
                    return account?.User;
                }
            });
        }

        public User GetUserById(int userId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    return context.Users.FirstOrDefault(u => u.UserId == userId);
                }
            });
        }

        public List<User> GetUsersByName(string name, int currentUserId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    var blockedUserIds = context.BlockedList
                        .Where(b => b.BlockerId == currentUserId)
                        .Select(b => b.BlockeeId)
                        .ToList();

                    var friendUserIds = context.Friendships
                        .Where(f => f.User1Id == currentUserId || f.User2Id == currentUserId)
                        .Select(f => f.User1Id == currentUserId ? f.User2Id : f.User1Id)
                        .ToList();

                    return context.Users.Include("Account")
                                      .Where(u => u.Username.Contains(name)
                                                  && u.UserId != currentUserId
                                                  && !blockedUserIds.Contains(u.UserId)
                                                  && !friendUserIds.Contains(u.UserId))
                                      .Take(20)
                                      .ToList();
                }
            });
        }
        public int UpdateUserNames(int userId, string username)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user == null)
                    {
                        return 0;
                    }

                    var existingUser = context.Users.FirstOrDefault(u => u.Username == username && u.UserId != userId);
                    if (existingUser != null)
                    {
                        throw new InvalidOperationException("Username already exists for another user.");
                    }

                    user.Username = username;
                    return context.SaveChanges();
                }
            });
        }


        public bool UpdateUser(User user)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    var existingUser = context.Users.FirstOrDefault(u => u.UserId == user.UserId);

                    if (existingUser == null)
                    {
                        return false; 
                    }

                    existingUser.Username = user.Username;
                    existingUser.ProfilePictureId = user.ProfilePictureId;

                    int alteredRows = context.SaveChanges();
                    return alteredRows > 0;
                }
            });
        }

        public bool UserExists(int userId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    return context.Users.Any(u => u.UserId == userId);
                }
            });
        }
    }
}
