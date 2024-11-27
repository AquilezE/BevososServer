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
        private void InvokeCallback(Action<IProfileManagerCallback> callbackAction, int userId)
        {
            IProfileManagerCallback callback = OperationContext.Current.GetCallbackChannel<IProfileManagerCallback>();
            try
            {
                callbackAction(callback);
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
                Disconnect(userId); 
            }
            catch (TimeoutException ex)
            {
                ExceptionManager.LogErrorException(ex);
                
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                Disconnect(userId); 
            }
        }

        public void ChangePassword(int userId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                InvokeCallback(cb => cb.OnPasswordChange("Password cannot be empty."), userId);
                return;
            }

            try
            {
                AccountDAO accountDAO = new AccountDAO();
                Account account = accountDAO.GetAccountByUserId(userId);

                if (account == null)
                {
                    InvokeCallback(cb => cb.OnPasswordChange("Account not found."), userId);
                    return;
                }

                if (!SimpleHashing.VerifyPassword(oldPassword, account.PasswordHash))
                {
                    InvokeCallback(cb => cb.OnPasswordChange("Incorrect password."), userId);
                    return;
                }

                string newHashedPassword = SimpleHashing.HashPassword(newPassword);
                bool result = accountDAO.UpdatePasswordByUserId(userId, newHashedPassword);

                InvokeCallback(
                    cb => cb.OnPasswordChange(result ? null : "Failed to update password."),
                    userId
                );
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                InvokeCallback(cb => cb.OnPasswordChange("An unexpected error occurred."), userId);
            }
        }

        public void UpdateProfile(int userId, string username, int profilePictureId)
        {


            try
            {
                UserDAO userDAO = new UserDAO();
                User user = userDAO.GetUserById(userId);

                if (user == null)
                {
                    InvokeCallback(cb => cb.OnProfileUpdate("",profilePictureId,"User not found."), userId);
                    return;
                }


                if (userDAO.UsernameExists(username))
                {
                    user.ProfilePictureId = profilePictureId;
                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        InvokeCallback(cb => cb.OnProfileUpdate("Not changed", profilePictureId, "Username exists"), userId);
                    }
                }
                else if (username == "Not changed")
                {
                    user.ProfilePictureId = profilePictureId;
                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        InvokeCallback(cb => cb.OnProfileUpdate(username, profilePictureId, ""), userId);
                    }
                }
                else
                {
                    user.Username = username;
                    user.ProfilePictureId = profilePictureId;

                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        InvokeCallback(cb => cb.OnProfileUpdate(username, profilePictureId, ""), userId);
                    }
                }
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
            }
        }
    }
}
