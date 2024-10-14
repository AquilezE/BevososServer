using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class AccountDAO
    {
        public Account GetAccountByEmail(string email)
        {
            using (var context = new BevososContext())
            {
                return context.Accounts.Include(a => a.User)
                                       .FirstOrDefault(a => a.Email == email);
            }
        }

        public bool EmailExists(string email)
        {
            using (var context = new BevososContext())
            {
                return context.Accounts.Any(a => a.Email == email);
            }
        }

        public void AddAccount(Account account)
        {
            using (var context = new BevososContext())
            {
                context.Accounts.Add(account);
                context.SaveChanges();
            }
        }

        public bool UpdatePasswordByEmail(string email, string newHashedPassword)
        {
            using (var context = new BevososContext())
            {
                var account = context.Accounts.FirstOrDefault(a => a.Email == email);
                account.PasswordHash = newHashedPassword;
                int alteredRows = context.SaveChanges();
                return alteredRows == 1;
            }
        }




    }
}
