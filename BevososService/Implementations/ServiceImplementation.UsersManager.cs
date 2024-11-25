using BevososService.DTOs;
using BevososService.Utils;
using DataAccess.DAO;
using DataAccess.Exceptions;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BevososService.Utils.Hasher;

namespace BevososService.Implementations
{
    public partial class ServiceImplementation : IUsersManager
    {
        public bool IsEmailTaken(string email)
        {
            try
            {
                return new AccountDAO().EmailExists(email);
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }

        public bool IsUsernameTaken(string username)
        {
            try
            {
                return new UserDAO().UsernameExists(username);
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }

        public bool RegisterUser(string email, string username, string password)
        {
            User user = new User();
            user.Username = username;

            Account account = new Account();
            account.Email = email;
            account.PasswordHash = SimpleHashing.HashPassword(password);

            try
            {
                return new AccountDAO().AddUserWithAccount(user, account);
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }

        public bool SendToken(string email)
        {
            try
            {
                TokenDAO tokenDao = new TokenDAO();
                if (tokenDao.HasToken(email))
                {
                    return EmailUtils.SendTokenByEmail(email, tokenDao.GetToken(email));
                }
                else
                {
                    tokenDao.AsignToken(email);
                    return EmailUtils.SendTokenByEmail(email, tokenDao.GetToken(email));
                }
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }

        public bool VerifyToken(string email, string token)
        {
            try
            {
                TokenDAO tokenDao = new TokenDAO();
                if (tokenDao.HasToken(email) && tokenDao.TokenIsValid(token, email))
                {
                    tokenDao.DeleteToken(token, email);
                    return true;
                }
                return false;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }
        public UserDto LogIn(string email, string password)
        {
            AccountDAO accountDAO = new AccountDAO();
            UserDAO userDAO = new UserDAO();

            try
            {
                Account account = accountDAO.GetAccountByEmail(email);

                if (account == null)
                {
                    return null;
                }

                if (SimpleHashing.VerifyPassword(password, account.PasswordHash))
                {
                    User user = userDAO.GetUserById(account.UserId);

                    UserDto userDto = new UserDto
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = account.Email,
                        ProfilePictureId = user.ProfilePictureId
                    };

                    return userDto;
                }
                return null;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }

        }

        public bool RecoverPassword(string email, string password)
        {
            try
            {
                AccountDAO accountDAO = new AccountDAO();
                if (!accountDAO.EmailExists(email))
                {
                    return false;
                }
                else
                {
                    string hashedPassword = SimpleHashing.HashPassword(password);
                    return accountDAO.UpdatePasswordByEmail(email, hashedPassword);
                }
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }

    }


}
