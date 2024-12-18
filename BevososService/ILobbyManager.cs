using System.Collections.Generic;
using BevososService.DTOs;
using System.ServiceModel;


namespace BevososService
{

    [ServiceContract(CallbackContract = typeof(ILobbyManagerCallback))]
    public interface ILobbyManager
    {

        /// <summary>
        /// Creates a new lobby and registers the creator as the lobby leader. Notifies the creator about the new lobby creation.
        /// </summary>
        /// <param name="userDto">The data transfer object containing information about the user creating the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void NewLobbyCreated(UserDTO userDto);

        /// <summary>
        /// Allows a user to join an existing lobby. Updates the current lobby state and notifies all existing members of the new joiner.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="userDto">The data transfer object with details of the user joining the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void JoinLobby(int lobbyId, UserDTO userDto);

        /// <summary>
        /// Removes a user from a specified lobby. Handles user departure logic and informs the remaining members of the update.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="userId">The unique identifier of the user leaving the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void LeaveLobby(int lobbyId, int userId);


        /// <summary>
        /// Sends a message to all users in the specified lobby. Each user receives a notification with the message content.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="userId">The unique identifier of the sender.</param>
        /// <param name="message">The content of the message to be sent.</param>
        [OperationContract(IsOneWay = true)]
        void SendMessage(int lobbyId, int userId, string message);


        /// <summary>
        /// Kicks a specified user from the lobby if the action is performed by the lobby leader. Notifies the kicked user with the reason.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="kickerId">The unique identifier of the user initiating the kick.</param>
        /// <param name="targetUserId">The unique identifier of the user being kicked.</param>
        /// <param name="reason">The reason for kicking the user.</param>
        [OperationContract(IsOneWay = true)]
        void KickUser(int lobbyId, int kickerId, int targetUserId, string reason);


        /// <summary>
        /// Initiates the start of a game for the specified lobby. Notifies all lobby members that the game has started.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void StartGame(int lobbyId);


        /// <summary>
        /// Changes the ready status of a user in the specified lobby. Notifies all lobby members when a user's ready status has changed.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="userId">The unique identifier of the user whose ready status has changed.</param>
        [OperationContract(IsOneWay = true)]
        void ChangeReadyStatus(int lobbyId, int userId);

    }

    [ServiceContract]
    internal interface ILobbyManagerCallback
    {

        /// <summary>
        /// Notifies the creator that a new lobby has been successfully created.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the newly created lobby.</param>
        /// <param name="userId">The unique identifier of the user who created the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void OnNewLobbyCreated(int lobbyId, int userId);

        /// <summary>
        /// Updates the client with the current list of users in the lobby.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="existingUsers">A list of `UserDTO` representing the current users in the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void OnLobbyUsersUpdate(int lobbyId, List<UserDTO> existingUsers);

        /// <summary>
        /// Notifies all members in the lobby when the lobby leader has changed.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="newLeaderId">The unique identifier of the new lobby leader.</param>
        [OperationContract(IsOneWay = true)]
        void OnLeaderChanged(int lobbyId, int newLeaderId);


        /// <summary>
        /// Notifies the lobby members when a new user has joined the lobby.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="userDto">The data transfer object of the user who joined the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void OnJoinLobby(int lobbyId, UserDTO userDto);


        /// <summary>
        /// Notifies the lobby members when a user has left the lobby.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <param name="userId">The unique identifier of the user who left the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void OnLeaveLobby(int lobbyId, int userId);

        /// <summary>
        /// Notifies the user that they have been kicked from the lobby and provides the reason.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby from which the user was kicked.</param>
        /// <param name="reason">The reason for the user being kicked.</param>
        [OperationContract(IsOneWay = true)]
        void OnKicked(int lobbyId, string reason);


        /// <summary>
        /// Sends a message notification to all users in the lobby.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who sent the message.</param>
        /// <param name="message">The content of the message.</param>
        [OperationContract(IsOneWay = true)]
        void OnSendMessage(int userId, string message);


        ///<summary>
        /// Notifies all lobby members when a user's ready status has changed.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose ready status has changed.</param>
        /// <param name="isReady">The new ready status of the user.</param>
        [OperationContract(IsOneWay = true)]
        void OnReadyStatusChanged(int userId, bool isReady);


        /// <summary>
        /// Notifies all lobby members that the game has started, initiating the transition from the lobby phase to the gameplay phase.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game session that has started.</param>
        [OperationContract(IsOneWay = true)]
        void GameStarted(int gameId);

    }

    [ServiceContract]
    public interface ILobbyChecker
    {

        /// <summary>
        /// Checks if a lobby is currently open and active.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <returns>True if the lobby is open, false otherwise.</returns>
        [OperationContract]
        bool IsLobbyOpen(int lobbyId);

        /// <summary>
        /// Checks if the specified lobby is full based on its current member count.
        /// </summary>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        /// <returns>True if the lobby has reached its maximum capacity, false otherwise.</returns>
        [OperationContract]
        bool IsLobbyFull(int lobbyId);

    }

}