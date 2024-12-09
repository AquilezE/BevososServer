using DataAccess.DAO;
using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;
using System;
using System.ServiceModel;
using static BevososService.Utils.Hasher;

namespace BevososService.Implementations
{
    //NEEDS STEROID CALLBACK EXCEPTION HANDLING
    public partial class ServiceImplementation : IProfileManager
    {
        public void ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IProfileManagerCallback>();
            try
            {
                if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
                {
                    callback.OnPasswordChange("Password cannot be empty.");
                }

                var accountDAO = new AccountDAO();
                Account account = accountDAO.GetAccountByUserId(userId);

                if (account == null)
                {
                    callback.OnPasswordChange("Account not found.");
                    return;
                }

                if (!SimpleHashing.VerifyPassword(oldPassword, account.PasswordHash))
                {
                    callback.OnPasswordChange("Incorrect password.");
                    return;
                }

                string newHashedPassword = SimpleHashing.HashPassword(newPassword);
                bool result = accountDAO.UpdatePasswordByUserId(userId, newHashedPassword);

                callback.OnPasswordChange(result ? null : "Failed to update password.");
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
                callback.OnPasswordChange("An unexpected error occurred.");
            }
            catch(TimeoutException ex)
            {
                ExceptionManager.LogErrorException(ex);
                callback.OnPasswordChange("An unexpected error occurred.");
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                callback.OnPasswordChange("An unexpected error occurred.");
            }
        }

        public void UpdateProfile(int userId, string username, int profilePictureId)
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IProfileManagerCallback>();
                var userDAO = new UserDAO();
                User user = userDAO.GetUserById(userId);

                if (user == null)
                {
                    callback.OnProfileUpdate("", profilePictureId, "User not found.");
                    return;
                }


                if (userDAO.UsernameExists(username))
                {
                    user.ProfilePictureId = profilePictureId;
                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate("Not changed", profilePictureId, "Username exists");
                    }
                }
                else if (username == "Not changed")
                {
                    user.ProfilePictureId = profilePictureId;
                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate(username, profilePictureId, "");
                    }
                }
                else
                {
                    user.Username = username;
                    user.ProfilePictureId = profilePictureId;

                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate(username, profilePictureId, "");
                    }
                }
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
                Disconnect(userId);
            }
            catch (TimeoutException ex)
            {
                ExceptionManager.LogErrorException(ex);
                Disconnect(userId);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                Disconnect(userId);
            }
        }
    }
}
