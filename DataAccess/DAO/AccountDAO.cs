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
                try
                {
                    using (var context = new BevososContext())
                    {
                        return context.Accounts.Include(a => a.User)
                                               .FirstOrDefault(a => a.UserId == accountId);
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

            public Account GetAccountByEmail(string email)
            {
                try
                {
                    using (var context = new BevososContext())
                    {
                        return context.Accounts.Include(a => a.User)
                                               .FirstOrDefault(a => a.Email == email);
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

            public bool EmailExists(string email)
            {
                try
                {
                    using (var context = new BevososContext())
                    {
                        return context.Accounts.Any(a => a.Email == email);
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

            public bool AddUserWithAccount(User user, Account account)
            {
                try
                {
                    using (var context = new BevososContext())
                    {
                        context.Accounts.Add(account);
                        context.Users.Add(user);
                        account.User = user;
                        account.UserId = user.UserId;

                        int alteredRows = context.SaveChanges();
                        return alteredRows == 2;
                    }
                }
                catch (DbUpdateException ex)
                {
                    ExceptionManager.LogErrorException(ex);
                    throw new DataBaseException(ex.Message);
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

            public bool UpdatePasswordByEmail(string email, string newHashedPassword)
            {
                try
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

            public bool UpdatePasswordByUserId(int userId, string newHashedPassword)
            {
                try
                {
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
                    ExceptionManager.LogErrorException(ex);
                    throw new DataBaseException(ex.Message);
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
