using BevososService.DTOs;
using BevososService.Exceptions;
using BevososService.Utils;
using DataAccess.DAO;
using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.Implementations
{

    //NEEDS STEROID CALLBACK EXCEPTION HANDLING

    public partial class ServiceImplementation : ISocialManager
    {

        private static readonly ConcurrentDictionary<int, ISocialManagerCallback> connectedClients = new ConcurrentDictionary<int, ISocialManagerCallback>();

        private readonly object _lock = new object();

        public void Connect(int userId)
        {
            try
            {


                var callback = OperationContext.Current.GetCallbackChannel<ISocialManagerCallback>();
                ICommunicationObject clientChannel = (ICommunicationObject)callback;

                Console.WriteLine("Client connected: " + userId);

                clientChannel.Closed += ClientChannel_Closed;
                clientChannel.Faulted += ClientChannel_Closed;

                lock (_lock)
                {
                    connectedClients.TryAdd(userId, callback);
                }

                NotifyFriendsUserOnline(userId);
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
            }
        }

        public void Disconnect(int userId)
        {
            lock (_lock)
            {
                connectedClients.TryRemove(userId, out _);
            }

            NotifyFriendsUserOffline(userId);
        }

        private void ClientChannel_Closed(object sender, EventArgs e)
        {
            var callback = (ISocialManagerCallback)sender;
            RemoveClient(callback);
        }

        private void RemoveClient(ISocialManagerCallback callback)
        {
            var user = connectedClients.FirstOrDefault(pair => pair.Value == callback);
            if (user.Key != 0)
            {
                Disconnect(user.Key);
            }
        }

        private void NotifyFriendsUserOnline(int userId)
        {
            Console.WriteLine("User online: " + userId);
            var friends = GetFriendIds(userId);
            foreach (var friendId in friends)
            {
                Console.WriteLine("Notifying friend: " + friendId);

                if (connectedClients.TryGetValue(friendId, out var friendCallback))
                {
                    try
                    {

                        friendCallback.OnFriendOnline(userId);
                        Console.WriteLine("Notified friend: " + friendId);
                    }
                    catch (Exception)
                    {
                        // Handle exceptions and possibly remove the faulty client
                    }
                }
            }
        }

        private void NotifyFriendsUserOffline(int userId)
        {
            var friends = GetFriendIds(userId);
            foreach (var friendId in friends)
            {
                if (connectedClients.TryGetValue(friendId, out var friendCallback))
                {
                    try
                    {
                        friendCallback.OnFriendOffline(userId);
                    }
                    catch (Exception)
                    {
                        // Handle exceptions and possibly remove the faulty client
                    }
                }
            }
        }

        private List<int> GetFriendIds(int userId)
        {
            var friends = GetFriends(userId);
            return friends.Select(f => f.FriendId).ToList();
        }

        public bool DeleteFriend(int userId, int friendId)
        {
            if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(friendId))
            {
                if (connectedClients.TryGetValue(friendId, out var friendCallback))
                {
                    friendCallback.OnFriendshipDeleted(userId);
                }
                return new FriendshipDAO().RemoveFriendship(userId, friendId);
            }
            return false;
        }


        public bool BlockFriend(int userId, int friendId)
        {
            if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(friendId))
            {
                bool result = new FriendshipDAO().RemoveFriendship(userId, friendId);
                if (result)
                {
                    if (connectedClients.TryGetValue(friendId, out var friendCallback))
                    {
                        friendCallback.OnFriendshipDeleted(userId);
                    }
                    return new BlockedDAO().AddBlock(userId, friendId);

                }
            }
            return false;
        }

        public List<BlockedDTO> GetBlockedUsers(int userId)
        {
            try
            {
                if (new UserDAO().UserExists(userId))
                {
                    List<BlockedData> blockedUsersList = new BlockedDAO().GetBlockedListForUser(userId);
                    List<BlockedDTO> blockedUsers = new List<BlockedDTO>();
                    foreach (BlockedData blockedUser in blockedUsersList)
                    {
                        blockedUsers.Add((BlockedDTO)blockedUser);
                    }
                    return blockedUsers;
                }
                return null;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }

        }

        public List<FriendRequestDTO> GetFriendRequests(int userId)
        {
            try
            {
                if (new UserDAO().UserExists(userId))
                {
                    List<FriendRequestData> friendRequestsList = new FriendRequestDAO().GetFriendRequestForUser(userId);
                    List<FriendRequestDTO> friendRequests = new List<FriendRequestDTO>();
                    foreach (FriendRequestData friendRequest in friendRequestsList)
                    {
                        friendRequests.Add((FriendRequestDTO)friendRequest);
                    }
                    return friendRequests;
                }
                return null;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }

        public List<FriendDTO> GetFriends(int userId)
        {
            try
            {
                if (new UserDAO().UserExists(userId))
                {
                    List<FriendData> friendshipList = new FriendshipDAO().GetFriendsForUser(userId);
                    List<FriendDTO> friends = new List<FriendDTO>();
                    foreach (FriendData friend in friendshipList)
                    {
                        if (connectedClients.TryGetValue(friend.FriendId, out _))
                        {
                            friend.IsConnected = true;
                        }

                        friends.Add((FriendDTO)friend);
                    }
                    return friends;
                }
                return null;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }

        }

        public bool SendFriendRequest(int userId, int requesteeId)
        {
            int idFriendRequest = new FriendRequestDAO().SendFriendRequest(userId, requesteeId);

            if (connectedClients.ContainsKey(requesteeId))
            {
                FriendRequestDTO friendRequest = new FriendRequestDTO();
                User sender = new UserDAO().GetUserById(userId);
                friendRequest.SenderId = userId;
                friendRequest.SenderName = sender.Username;
                friendRequest.ProfilePictureId = sender.ProfilePictureId;
                friendRequest.FriendRequestId = idFriendRequest;

                connectedClients[requesteeId].OnNewFriendRequest(friendRequest);
                return true;
            }
            return 0 != idFriendRequest;
        }

        public void InviteFriendToLobby(string inviterName, int userId, int lobbyId)
        {
            if (activeLobbiesDict.ContainsKey(lobbyId))
            {
                if (connectedClients.TryGetValue(userId, out ISocialManagerCallback callback))
                {
                    callback.NotifyGameInvited(inviterName, lobbyId);
                    return;
                }


                Account account = new AccountDAO().GetAccountByUserId(userId);
                if (account != null)
                {
                    EmailUtils.SendInvitationByEmail(account.Email, lobbyId);
                }

            }
        }

        public void AcceptFriendRequest(int userId, int friendId, int requestId)
        {
            try
            {
                if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(friendId))
                {
                    bool result = new FriendRequestDAO().AcceptFriendRequest(requestId);
                    if (result)
                    {
                        Friendship friendship = new FriendshipDAO().AddFriendship(userId, friendId);
                        if (friendship != null)
                        {
                            int friendshipId = friendship.Id;

                            var userDao = new UserDAO();
                            var currentUser = userDao.GetUserById(userId);
                            var friendUser = userDao.GetUserById(friendId);

                            var friendDto = new FriendDTO
                            {
                                FriendshipId = friendshipId,
                                FriendId = friendId,
                                FriendName = friendUser.Username,
                                ProfilePictureId = friendUser.ProfilePictureId,
                                IsConnected = connectedClients.ContainsKey(friendId)
                            };

                            if (connectedClients.TryGetValue(userId, out var userCallback))
                            {
                                try
                                {
                                    userCallback.OnNewFriend(friendDto);
                                }
                                catch (CommunicationException ex)
                                {
                                    ExceptionManager.LogErrorException(ex);
                                    connectedClients.TryRemove(userId, out _);
                                }
                                catch (TimeoutException ex)
                                {
                                    ExceptionManager.LogErrorException(ex);
                                }
                                catch (Exception ex)
                                {
                                    ExceptionManager.LogFatalException(ex);
                                }
                            }

                            var friendDtoForFriend = new FriendDTO
                            {
                                FriendshipId = friendshipId,
                                FriendId = userId,
                                FriendName = currentUser.Username,
                                ProfilePictureId = currentUser.ProfilePictureId,
                                IsConnected = connectedClients.ContainsKey(userId)
                            };


                            if (connectedClients.TryGetValue(friendId, out var friendCallback))
                            {
                                try
                                {
                                    friendCallback.OnNewFriend(friendDtoForFriend);
                                }
                                catch (CommunicationException ex)
                                {
                                    ExceptionManager.LogErrorException(ex);
                                    connectedClients.TryRemove(userId, out _);

                                }
                                catch (TimeoutException ex)
                                {
                                    ExceptionManager.LogErrorException(ex);
                                }
                                catch (Exception ex)
                                {
                                    ExceptionManager.LogFatalException(ex);

                                }

                                return;
                            }
                        }
                    }
                }
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }
        public bool DeclineFriendRequest(int requestId)
        {
            try
            {
                return new FriendRequestDAO().DeclineFriendRequest(requestId);
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }
        public bool UnblockUser(int userId, int blockedId)
        {
            try
            {
                if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(blockedId))
                {
                    return new BlockedDAO().DeleteBlock(userId, blockedId);
                }
                return false;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }

        public List<UserDto> GetUsersFoundByName(int userId, string name)
        {
            var users = new List<UserDto>();
            var usersData = new UserDAO().GetUsersByName(name, userId);
            foreach (var user in usersData)
            {
                users.Add((UserDto)user);
            }
            return users;
        }

        public bool BlockUser(int userId, int blockeeId)
        {
            try
            {
                if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(blockeeId))
                {
                    if (new BlockedDAO().AddBlock(userId, blockeeId))
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }

        public bool IsConnected(string email)
        {
            try
            {
                User user = new UserDAO().GetUserByEmail(email);
                if (user != null)
                {
                    return connectedClients.ContainsKey(user.UserId);
                }
                return false;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }
    }
}
