using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;

namespace DataAccess.DAO
{
    public class UserDAO
    {
        public bool UsernameExists(string username)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    return context.Users.Any(u => u.Username == username);
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public User GetUserByEmail(string email)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var account = context.Accounts
                                         .Include("User") // Eager loading the related User
                                         .FirstOrDefault(a => a.Email == email);

                    return account?.User; // Return the User if the account is found, otherwise null
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public User GetUserById(int userId)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    return context.Users.FirstOrDefault(u => u.UserId == userId);
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public List<User> GetUsersByName(string name, int currentUserId)
        {
            try
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
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public int UpdateUserNames(int userId, string username)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    if (user == null)
                    {
                        return 0;
                    }

                    var existingUserWithSameUsername = context.Users.FirstOrDefault(u => u.Username == username && u.UserId != userId);

                    if (existingUserWithSameUsername != null)
                    {
                        throw new InvalidOperationException("Username already exists for another user.");
                    }

                    user.Username = username;

                    return context.SaveChanges();
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public bool UpdateUser(User user)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    context.Entry(user).State = EntityState.Modified;
                    int alteredRows = context.SaveChanges();
                    return alteredRows == 1;
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ExceptionManager.LogErrorException(ex);
                return false;
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                return false;
            }
        }

        public bool UserExists(int userId)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    return context.Users.Any(u => u.UserId == userId);
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }
    }
}
