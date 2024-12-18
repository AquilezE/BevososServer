using System.ServiceModel;
using BevososService.DTOs;
using BevososService.Exceptions;

namespace BevososService
{

    [ServiceContract]
    internal interface IUsersManager
    {

        /// <summary>
        /// Checks if the provided username is already in use by another user.
        /// </summary>
        /// <param name="username">The username to be checked.</param>
        /// <returns>True if the username is already taken, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool IsUsernameTaken(string username);


        /// <summary>
        /// Checks if the provided email is already registered in the system.
        /// </summary>
        /// <param name="email">The email address to be checked.</param>
        /// <returns>True if the email is already taken, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool IsEmailTaken(string email);


        /// <summary>
        /// Sends a verification token to the provided email if it exists in the system. Generates and assigns a new token if none exists.
        /// </summary>
        /// <param name="email">The email address to send the token to.</param>
        /// <returns>True if the token was successfully sent, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool SendToken(string email);


        /// <summary>
        /// Verifies if the provided token is valid for the given email. Deletes the token if valid.
        /// </summary>
        /// <param name="email">The email address associated with the token.</param>
        /// <param name="token">The token to be verified.</param>
        /// <returns>True if the token is valid, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool VerifyToken(string email, string token);


        /// <summary>
        /// Registers a new user with the provided email, username, and password. The password is hashed before being stored.
        /// </summary>
        /// <param name="email">The email address of the new user.</param>
        /// <param name="username">The chosen username for the new user.</param>
        /// <param name="password">The password for the new user, which will be hashed before storage.</param>
        /// <returns>True if the registration was successful, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool RegisterUser(string email, string username, string password);


        /// <summary>
        /// Authenticates a user using their email and password. Returns a UserDTO containing user details if successful.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password provided by the user.</param>
        /// <returns>A `UserDTO` containing user details if authentication is successful, otherwise null.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        UserDTO LogIn(string email, string password);

        /// <summary>
        /// Retrieves the guest user. This user is used for anonymous access to certain features.
        /// </summary>
        /// <returns>A UserDTO representing the guest user.</returns>
        [OperationContract]
        UserDTO GetGuestUser();


        /// <summary>
        /// Updates the password for a user based on their email. The new password is hashed before being stored.
        /// </summary>
        /// <param name="email">The email address of the user whose password is being updated.</param>
        /// <param name="password">The new password to be set, which will be hashed before storage.</param>
        /// <returns>True if the password was successfully updated, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool RecoverPassword(string email, string password);

    }

}