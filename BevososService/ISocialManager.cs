using BevososService.DTOs;
using BevososService.Exceptions;
using System.Collections.Generic;
using System.ServiceModel;


namespace BevososService
{
    [ServiceContract(CallbackContract = typeof(ISocialManagerCallback))]
    internal interface ISocialManager
    {
        /// <summary>
        /// Connects a user to the social service, registering their callback for notifications and notifying their friends that they are online.
        /// </summary>
        /// <param name="userId">The unique identifier of the user connecting to the service.</param>
        [OperationContract(IsOneWay = true)]
        void Connect(int userId);


        /// <summary>
        /// Disconnects a user from the social service, removing their callback and notifying their friends that they are offline.
        /// </summary>
        /// <param name="userId">The unique identifier of the user disconnecting from the service.</param>
        [OperationContract(IsOneWay = true)]
        void Disconnect(int userId);


        /// <summary>
        /// Checks if a user is currently connected based on their email.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <returns>True if the user is connected, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool IsConnected(string email);


        /// <summary>
        /// Sends a friend request from one user to another. If the recipient is connected, notifies them in real-time.
        /// </summary>
        /// <param name="userId">The unique identifier of the user sending the friend request.</param>
        /// <param name="requesteeId">The unique identifier of the recipient.</param>
        /// <returns>True if the request was successfully sent, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool SendFriendRequest(int userId, int requesteeId);


        /// <summary>
        /// Accepts a friend request and adds the sender and recipient to each other's friend lists.
        /// </summary>
        /// <param name="userId">The unique identifier of the recipient of the friend request.</param>
        /// <param name="friendId">The unique identifier of the sender of the friend request.</param>
        /// <param name="requestId">The unique identifier of the friend request.</param>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool AcceptFriendRequest(int userId, int friendId, int requestId);

        /// <summary>
        /// Declines a pending friend request.
        /// </summary>
        /// <param name="requestId">The unique identifier of the friend request to be declined.</param>
        /// <returns>True if the request was successfully declined, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool DeclineFriendRequest(int requestId);


        /// <summary>
        /// Deletes a friend from the user's friend list. Notifies the removed friend if they are connected.
        /// </summary>
        /// <param name="userId">The unique identifier of the user performing the deletion.</param>
        /// <param name="friendId">The unique identifier of the friend being deleted.</param>
        /// <returns>True if the friend was successfully deleted, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool DeleteFriend(int userId, int friendId);

        /// <summary>
        /// Retrieves a list of friends for the specified user, indicating whether each friend is currently connected.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of friends as `FriendDTO` objects, or null if the user does not exist.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        List<FriendDTO> GetFriends(int userId);

        /// <summary>
        /// Blocks a friend and removes them from the user's friend list. Notifies the blocked friend if they are connected.
        /// </summary>
        /// <param name="userId">The unique identifier of the user blocking the friend.</param>
        /// <param name="friendId">The unique identifier of the friend being blocked.</param>
        /// <returns>True if the friend was successfully blocked, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool BlockFriend(int userId, int friendId);

        /// <summary>
        /// Unblocks a previously blocked user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user performing the unblock action.</param>
        /// <param name="blockedId">The unique identifier of the blocked user.</param>
        /// <returns>True if the user was successfully unblocked, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool UnblockUser(int userId, int blockedId);


        /// <summary>
        /// Blocks a user without removing them from the friend list.
        /// </summary>
        /// <param name="userId">The unique identifier of the user performing the block action.</param>
        /// <param name="blockeeId">The unique identifier of the user being blocked.</param>
        /// <returns>True if the user was successfully blocked, false otherwise.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        bool BlockUser(int userId, int blockeeId);

        /// <summary>
        /// Retrieves a list of blocked users for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of blocked users as `BlockedDTO` objects, or null if the user does not exist.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        List<BlockedDTO> GetBlockedUsers(int userId);


        /// <summary>
        /// Retrieves a list of friend requests for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of friend requests as `FriendRequestDTO` objects, or null if the user does not exist.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        List<FriendRequestDTO> GetFriendRequests(int userId);

        /// <summary>
        /// Searches for users by name and returns a list of matching users.
        /// </summary>
        /// <param name="userId">The unique identifier of the user performing the search.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>A list of matching `UserDTO` objects.</returns>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        List<UserDTO> GetUsersFoundByName(int userId, string name);


        /// <summary>
        /// Sends an invitation to a user to join a lobby. If the user is connected, notifies them directly; otherwise, sends an email.
        /// </summary>
        /// <param name="inviterName">The name of the inviter.</param>
        /// <param name="userId">The unique identifier of the user being invited.</param>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void InviteFriendToLobby(string inviterName, int userId, int lobbyId);
    }

    [ServiceContract]
    internal interface ISocialManagerCallback
    {
        /// <summary>
        /// Notifies the client that one of their friends has come online.
        /// </summary>
        /// <param name="userId">The unique identifier of the friend who is now online.</param>
        [OperationContract(IsOneWay = true)]
        void OnFriendOnline(int userId);


        /// <summary>
        /// Notifies the client that one of their friends has gone offline.
        /// </summary>
        /// <param name="userId">The unique identifier of the friend who is now offline.</param>
        [OperationContract(IsOneWay = true)]
        void OnFriendOffline(int userId);

        /// <summary>
        /// Notifies the client when a new friendship has been accepted and added.
        /// </summary>
        /// <param name="friendDto">The data transfer object representing the new friend.</param>
        [OperationContract(IsOneWay = true)]
        void OnNewFriend(FriendDTO friendDto);

        /// <summary>
        /// Notifies the client that a friendship has been removed.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who removed the friendship.</param>
        [OperationContract(IsOneWay = true)]
        void OnFriendshipDeleted(int userId);

        /// <summary>
        /// Notifies the client when they receive a new friend request.
        /// </summary>
        /// <param name="friendRequest">The data transfer object containing details of the friend request.</param>
        [OperationContract(IsOneWay = true)]
        void OnNewFriendRequest(FriendRequestDTO friendRequest);

        /// <summary>
        /// Notifies the client that they have been invited to a game lobby by a friend.
        /// </summary>
        /// <param name="inviterName">The name of the friend who sent the invitation.</param>
        /// <param name="lobbyId">The unique identifier of the lobby.</param>
        [OperationContract(IsOneWay = true)]
        void NotifyGameInvited(string inviterName, int lobbyId);
    }
}