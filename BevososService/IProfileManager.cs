
using System.ServiceModel;


namespace BevososService
{
    [ServiceContract (CallbackContract = typeof ( IProfileManagerCallback))]

    internal interface IProfileManager
    {
        /// <summary>
        /// Updates the profile of a user, including the username and profile picture. Sends a callback indicating the result.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose profile is being updated.</param>
        /// <param name="username">The new username to be set.</param>
        /// <param name="profilePictureId">The identifier for the new profile picture.</param>
        [OperationContract(IsOneWay = true)]
        void UpdateProfile(int userId, string username, int profilePictureId);


        /// <summary>
        /// Changes the password for a user if the provided old password is correct. Sends a callback with the result of the operation.
        /// </summary>
        /// <param name="userId">The unique identifier of the user requesting the password change.</param>
        /// <param name="oldPassword">The current password for verification.</param>
        /// <param name="newPassword">The new password to be set.</param>
        [OperationContract(IsOneWay = true)]
        void ChangePassword(int userId, string oldPassword, string newPassword);

    }

    [ServiceContract]
    internal interface IProfileManagerCallback
    {
        /// <summary>
        /// Notifies the client about the result of a profile update operation.
        /// </summary>
        /// <param name="username">The updated username of the user, or "Not changed" if the username was not modified.</param>
        /// <param name="profilePictureId">The identifier of the updated profile picture.</param>
        /// <param name="message">A message indicating the outcome of the update, or an empty string if the update was successful.</param>
        [OperationContract(IsOneWay = true)]
        void OnProfileUpdate(string username, int profilePictureId, string message);


        /// <summary>
        /// Notifies the client of the result of a password change operation.
        /// </summary>
        /// <param name="result">A message indicating the result of the password change, or null if the change was successful.</param>
        [OperationContract(IsOneWay = true)]
        void OnPasswordChange(string result);

    }
}