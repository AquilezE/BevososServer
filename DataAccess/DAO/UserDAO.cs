using System;
using System.Collections.Generic;
using System.Linq;
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

        public void AddUser(User user)
        {
            using (var context = new BevososContext())
            {
                context.Users.Add(user);
                context.SaveChanges();
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




    }
}
