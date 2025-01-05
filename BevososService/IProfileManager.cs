using BevososService.Exceptions;
using System.ServiceModel;


namespace BevososService
{

    [ServiceContract]
    internal interface IProfileManager
    {

        /// <summary>
        /// Updates the profile of a user, including the username and profile picture. Sends a callback indicating the result.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose profile is being updated.</param>
        /// <param name="username">The new username to be set.</param>
        /// <param name="profilePictureId">The identifier for the new profile picture.</param>
        /// <returns>0 if the profile was successfully updated, 1 if the username is already in use, 2 if the user does not exist, 3 if an exception occurred.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        int UpdateProfile(int userId, string username, int profilePictureId);


        /// <summary>
        /// Changes the password for a user if the provided old password is correct. Sends a callback with the result of the operation.
        /// </summary>
        /// <param name="userId">The unique identifier of the user requesting the password change.</param>
        /// <param name="oldPassword">The current password for verification.</param>
        /// <param name="newPassword">The new password to be set.</param>
        /// <returns>0 if the password was successfully changed, 1 if the old password was incorrect,    if the user does not exist, 3 if an exception occurred.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        int ChangePassword(int userId, string oldPassword, string newPassword);

    }
}