﻿using DataAccess.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;



namespace DataAccess.DAO
{
    public class AccountDAO
    {
        public Account GetAccountByUserId(int accountId)
        {
            using (var context = new BevososContext())
            {
                return context.Accounts.Include(a => a.User)
                                       .FirstOrDefault(a => a.UserId == accountId);
            }
        }

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


        public bool AddUserWithAccount(User user, Account account)
        {
            using (var context = new BevososContext())
            {
                context.Accounts.Add(account);
                context.Users.Add(user);
                account.User = user;
                account.UserId = user.UserId;
                try
                {
                    int alteredRows = context.SaveChanges();
                    return alteredRows == 2;
                }
                catch (DbUpdateException ex)
                {
                    //ask revo to set up log4net :/
                    return false;
                }
            }
        }

        public bool UpdatePasswordByEmail(string email, string newHashedPassword)
        {
            using (var context = new BevososContext())
            {
                var account = context.Accounts.FirstOrDefault(a => a.Email == email);

                if (account == null)
                {
                    return false;
                }

                account.PasswordHash = newHashedPassword;
                int alteredRows = context.SaveChanges();
                return alteredRows == 1;
            }
        }

        public bool UpdatePasswordByUserId(int userId, string newHashedPassword)
        {
            try { 
            using (var context = new BevososContext())
            {

                var account = context.Accounts.FirstOrDefault(a => a.UserId == userId);

                if (account == null)
                {
                        return false;
                }

                account.PasswordHash = newHashedPassword;
                int alteredRows = context.SaveChanges();
                return alteredRows == 1;
            }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
