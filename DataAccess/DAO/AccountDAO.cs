using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;
using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;



namespace DataAccess.DAO
{
        public class AccountDAO
        {
        public Account GetAccountByUserId(int accountId)
        {
            return ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    return context.Accounts
                                  .Include(a => a.User)
                                  .FirstOrDefault(a => a.UserId == accountId);
                }
            });
        }

        public Account GetAccountByEmail(string email)
        {
            return ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    return context.Accounts
                                  .Include(a => a.User)
                                  .FirstOrDefault(a => a.Email == email);
                }
            });
        }

        public bool EmailExists(string email)
        {
            return ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    return context.Accounts.Any(a => a.Email == email);
                }
            });
        }

        public bool AddUserWithAccount(User user, Account account)
        {
            return ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
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
            return ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
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
            return ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
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

        private T ExecuteWithExceptionHandling<T>(Func<T> func)
        {
            try
            {
                return func();
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
