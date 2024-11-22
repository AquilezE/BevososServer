using DataAccess.DAO;
using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
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

            AccountDAO accountDAO = new AccountDAO();
            try
            {
                Account account = accountDAO.GetAccountByUserId(userId);

                if (SimpleHashing.VerifyPassword(oldPassword, account.PasswordHash))
                {
                    string newHashedPassword = SimpleHashing.HashPassword(newPassword);
                    bool result = accountDAO.UpdatePasswordByUserId(userId, newHashedPassword);


                    if (result)
                    {
                        InvokeCallback(cb => cb.OnPasswordChange(null), userId);
                    }
                    else
                    {
                        InvokeCallback(cb => cb.OnPasswordChange("Failed to update password."), userId);
                    }
                }
                else
                {
                    InvokeCallback(cb => cb.OnPasswordChange("Incorrect password."), userId);
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

        public void UpdateProfile(int userId, string username, int profilePictureId)
        {
            UserDAO userDAO = new UserDAO();
            User user = userDAO.GetUserById(userId);

            try
            {
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
                        InvokeCallback(cb => cb.OnProfileUpdate("Failed to update profile.", profilePictureId, "Username exists"), userId);
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
