using DataAccess.Exceptions;
using DataAccess.Utils;
using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;


namespace DataAccess.DAO
{
    public class TokenDAO
    {
        public int AsignToken(string email)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var existingToken = context.Tokens.FirstOrDefault(t => t.Email == email);
                    if (existingToken != null)
                    {
                        existingToken.TokenValue = new TokenGenerator().GenerateToken();
                        existingToken.ExpiryDate = DateTime.Now.AddMinutes(15);
                    }
                    else
                    {
                        var token = new Models.Token();
                        token.Email = email;
                        token.TokenValue = new TokenGenerator().GenerateToken();
                        token.ExpiryDate = DateTime.Now.AddMinutes(15);
                        context.Tokens.Add(token);
                    }

                    int affectedRows = context.SaveChanges();
                    return affectedRows;
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

        public bool HasToken(string email)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    return context.Tokens.Any(t => t.Email == email && t.ExpiryDate > DateTime.Now);
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

        public string GetToken(string email)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var token = context.Tokens.FirstOrDefault(t => t.Email == email);

                    if (token == null)
                    {
                        return "-1";
                    }

                    return token.TokenValue;
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

        public bool TokenIsValid(string token, string email)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    return context.Tokens.Any(t => t.TokenValue == token && t.ExpiryDate > DateTime.Now && t.Email == email);
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

        public bool DeleteToken(string token, string email)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var tokenToDelete = context.Tokens.FirstOrDefault(t => t.TokenValue == token && t.Email == email);

                    if (tokenToDelete != null)
                    {
                        context.Tokens.Remove(tokenToDelete);
                        int affectedRows = context.SaveChanges();
                        return affectedRows > 0;
                    }
                    return false;
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
