using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

namespace DataAccess.DAO
{
        public class UserDAO
    {

        public bool UsernameExists(string username)
        {
            using (var context = new BevososContext())
            {
                return context.Users.Any(u => u.Username == username);
            }
        }

        public User GetUserByEmail(string email)
        {
            using (var context = new BevososContext())
            {
                var account = context.Accounts
                                     .Include("User") // Eager loading the related User
                                     .FirstOrDefault(a => a.Email == email);

                return account?.User; // Return the User if the account is found, otherwise null
            }
        }


        public User GetUserById(int userId)
        {
            using (var context = new BevososContext())
            {
                return context.Users.FirstOrDefault(u => u.UserId == userId);
            }
        }

        public List<User> GetUsersByName(string name, int currentUserId)
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



        public int UpdateUserNames(int userId, string username)
        {
            using (var context = new BevososContext())
            {
                // Find the user by userId
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);

                // If the user is not found, return 0 (no rows affected)
                if (user == null)
                {
                    return 0;
                }

                // Check if the new username already exists for another user
                var existingUserWithSameUsername = context.Users.FirstOrDefault(u => u.Username == username && u.UserId != userId);

                if (existingUserWithSameUsername != null)
                {
                    throw new InvalidOperationException("Username already exists for another user.");
                }

                // Update the username
                user.Username = username;

                // Save the changes and return the number of rows affected
                return context.SaveChanges();
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
            catch (DbUpdateConcurrencyException e)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool UserExists(int userId)
        {
            using (var context = new BevososContext())
            {
                return context.Users.Any(u => u.UserId == userId);
            }
        }

    }
}
