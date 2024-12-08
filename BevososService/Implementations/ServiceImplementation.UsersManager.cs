using BevososService.DTOs;
using BevososService.Utils;
using DataAccess.DAO;
using DataAccess.Exceptions;
using DataAccess.Models;
using System.Threading;
using static BevososService.Utils.Hasher;

namespace BevososService.Implementations
{
    public partial class ServiceImplementation : IUsersManager
    {
        private static int _currentGuestId = -4;

        private static int GenerateUniqueGuestId()
        {
            return Interlocked.Decrement(ref _currentGuestId);
        }
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
            var user = new User
            {
                Username = username
            };

            var account = new Account
            {
                Email = email,
                PasswordHash = SimpleHashing.HashPassword(password)
            };

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
                var tokenDao = new TokenDAO();
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
                var tokenDao = new TokenDAO();
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
        public UserDTO LogIn(string email, string password)
        {
            var accountDAO = new AccountDAO();
            var userDAO = new UserDAO();

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

                    var userDto = new UserDTO
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

        public UserDTO GetGuestUser()
        {
            int guestId = GenerateUniqueGuestId();
            var user = new UserDTO
            {
                UserId = guestId,
                Username = "Guest" + guestId,
                Email = "guest" + guestId + "@bevosos.com",
                ProfilePictureId = 1
            };

            return user;
        }

        public bool RecoverPassword(string email, string password)
        {
            try
            {
                var accountDAO = new AccountDAO();
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
