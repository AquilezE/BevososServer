﻿using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;
using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;


namespace DataAccess.DAO
{

    public class AccountDAO
    {

        public Account GetAccountByUserId(int accountId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    return context.Accounts
                        .Include(a => a.User)
                        .FirstOrDefault(a => a.UserId == accountId);
                }
            });
        }

        public Account GetAccountByEmail(string email)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    return context.Accounts
                        .Include(a => a.User)
                        .FirstOrDefault(a => a.Email == email);
                }
            });
        }

        public bool EmailExists(string email)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    return context.Accounts.Any(a => a.Email == email);
                }
            });
        }

        public bool AddUserWithAccount(User user, Account account)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    context.Users.Add(user);
                    context.Accounts.Add(account);
                    account.User = user;
                    account.UserId = user.UserId;

                    int alteredRows = context.SaveChanges();
                    return alteredRows == 2;
                }
            });
        }

        public bool UpdatePasswordByEmail(string email, string newHashedPassword)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    Account account = context.Accounts.FirstOrDefault(a => a.Email == email);
                    if (account == null)
                    {
                        return false;
                    }

                    account.PasswordHash = newHashedPassword;
                    int alteredRows = context.SaveChanges();
                    return alteredRows == 1;
                }
            });
        }

        public bool UpdatePasswordByUserId(int userId, string newHashedPassword)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (var context = new BevososContext())
                {
                    Account account = context.Accounts.FirstOrDefault(a => a.UserId == userId);
                    if (account == null)
                    {
                        return false;
                    }

                    account.PasswordHash = newHashedPassword;
                    int alteredRows = context.SaveChanges();
                    return alteredRows == 1;
                }
            });
        }

    }

}