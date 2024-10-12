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
                return context.Users.FirstOrDefault(u => u.Email == email);
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


        public void UpdateUserNames(int userId, string username)
        {
            using (var context = new BevososContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                user.Username= username;
                context.SaveChanges();
            }
        }




    }
}
