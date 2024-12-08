﻿using BevososService.DTOs;
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

namespace BevososService.Implementations
{

    //NEEDS STEROID CALLBACK EXCEPTION HANDLING

    public partial class ServiceImplementation : ISocialManager
    {

        private static readonly ConcurrentDictionary<int, ISocialManagerCallback> ConnectedClients = new ConcurrentDictionary<int, ISocialManagerCallback>();

        private readonly object _lock = new object();


        public void Connect(int userId)
        {
            try
            {


                var callback = OperationContext.Current.GetCallbackChannel<ISocialManagerCallback>();
                var clientChannel = (ICommunicationObject)callback;

                Console.WriteLine("Client connected: " + userId);

                clientChannel.Closed += ClientSocialChannelRuined;
                clientChannel.Faulted += ClientSocialChannelRuined;

                lock (_lock)
                {
                    ConnectedClients.TryAdd(userId, callback);
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
                Disconnect(userId);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                Disconnect(userId);
            }
        }

        public void Disconnect(int userId)
        {
            lock (_lock)
            {
                ConnectedClients.TryRemove(userId, out _); Console.WriteLine("Disconnected: "+ userId);
            }

            NotifyFriendsUserOffline(userId);
        }

        public bool IsConnected(string email)
        {
            try
            {

                User user = new UserDAO().GetUserByEmail(email);
                if (user != null)
                {
                    return ConnectedClients.ContainsKey(user.UserId);
                }
                return false;
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
        }


        public bool SendFriendRequest(int userId, int requesteeId)
        {
            int idFriendRequest = new FriendRequestDAO().SendFriendRequest(userId, requesteeId);

            if (ConnectedClients.ContainsKey(requesteeId))
            {
                try
                {
                    var friendRequest = new FriendRequestDTO();
                    User sender = new UserDAO().GetUserById(userId);
                    friendRequest.SenderId = userId;
                    friendRequest.SenderName = sender.Username;
                    friendRequest.ProfilePictureId = sender.ProfilePictureId;
                    friendRequest.FriendRequestId = idFriendRequest;

                    InvokeCallback(requesteeId, cb => cb.OnNewFriendRequest(friendRequest));
                    return true;
                }
                catch (DataBaseException ex)
                {
                    throw CreateAndLogFaultException(ex);
                }
            }
            return 0 != idFriendRequest;
        }

        //TODO: FIX THIS UGLY AHH CODE ASAP 
        public bool AcceptFriendRequest(int userId, int friendId, int requestId)
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
                            User currentUser = userDao.GetUserById(userId);
                            User friendUser = userDao.GetUserById(friendId);

                            var friendDto = new FriendDTO
                            {
                                FriendshipId = friendshipId,
                                FriendId = friendId,
                                FriendName = friendUser.Username,
                                ProfilePictureId = friendUser.ProfilePictureId,
                                IsConnected = ConnectedClients.ContainsKey(friendId)
                            };

                            var friendDtoForFriend = new FriendDTO
                            {
                                FriendshipId = friendshipId,
                                FriendId = userId,
                                FriendName = currentUser.Username,
                                ProfilePictureId = currentUser.ProfilePictureId,
                                IsConnected = ConnectedClients.ContainsKey(userId)
                            };


                            InvokeCallback(userId, (callback, dto) => callback.OnNewFriend(dto), friendDto);

                            // Invoke callback for the friend
                            try
                            {

                                if (ConnectedClients.TryGetValue(friendId, out ISocialManagerCallback callback))
                                {
                                    try
                                    {
                                        callback.OnNewFriend(friendDtoForFriend);
                                    }
                                    catch (CommunicationException ex){
                                    

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
                            catch (CommunicationException ex)
                            {
                                Console.WriteLine("ComErr");
                            }
                            catch(TimeoutException ex)
                            {
                                Console.WriteLine("TErr");
                            } catch (Exception ex) {
                                Console.WriteLine("ErrG");
                            }

                            
                            //InvokeCallback(friendId, (callback, dto) => callback.OnNewFriend(dto), friendDtoForFriend);

                            return true;
                        }
                    }
                }
                return false;
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

        public List<FriendRequestDTO> GetFriendRequests(int userId)
        {
            try
            {
                if (new UserDAO().UserExists(userId))
                {
                    List<FriendRequestData> friendRequestsList = new FriendRequestDAO().GetFriendRequestForUser(userId);
                    var friendRequests = new List<FriendRequestDTO>();
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

        public bool DeleteFriend(int userId, int friendId)
        {
            try
            {
                var userDao = new UserDAO();
                var friendshipDao = new FriendshipDAO();

                if (userDao.UserExists(userId) && userDao.UserExists(friendId))
                {
                    bool result = friendshipDao.RemoveFriendship(userId, friendId);
                    if (result)
                    {
                        InvokeCallback(friendId, (callback, id) => callback.OnFriendshipDeleted(id), userId);
                        return true;
                    }
                }
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
            return false;
        }

        public List<FriendDTO> GetFriends(int userId)
        {
            try
            {
                if (new UserDAO().UserExists(userId))
                {
                    List<FriendData> friendshipList = new FriendshipDAO().GetFriendsForUser(userId);
                    var friends = new List<FriendDTO>();
                    foreach (FriendData friend in friendshipList)
                    {
                        if (ConnectedClients.TryGetValue(friend.FriendId, out _))
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


        public bool BlockFriend(int userId, int friendId)
        {
            try
            {
                if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(friendId))
                {
                    bool resultFriendDeleted = new FriendshipDAO().RemoveFriendship(userId, friendId);
                    if (resultFriendDeleted)
                    {
                        bool resultBlockCreated = new BlockedDAO().AddBlock(userId, friendId);
                        if (resultBlockCreated)
                        {
                            InvokeCallback(friendId, (callback, id) => callback.OnFriendshipDeleted(id), userId);
                            return true;
                        }
                    }
                }
                return false;
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

        public List<BlockedDTO> GetBlockedUsers(int userId)
        {
            try
            {
                if (new UserDAO().UserExists(userId))
                {
                    List<BlockedData> blockedUsersList = new BlockedDAO().GetBlockedListForUser(userId);
                    var blockedUsers = new List<BlockedDTO>();
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




        private void InvokeCallback<T>(int userId, Action<ISocialManagerCallback, T> callbackAction, T parameter)
        {
            if (ConnectedClients.TryGetValue(userId, out ISocialManagerCallback callback))
            {
                try
                {
                    callbackAction(callback, parameter);
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
        }

        private void InvokeCallback(int userId, Action<ISocialManagerCallback> callbackAction)
        {
            if (ConnectedClients.TryGetValue(userId, out ISocialManagerCallback callback))
            {
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
                    Disconnect(userId);
                }
                catch (Exception ex)
                {
                    ExceptionManager.LogFatalException(ex);
                    Disconnect(userId);
                }
            }
        }


        private void NotifyFriendsUserOnline(int userId)
        {
            List<int> friends = GetFriendIds(userId);
            foreach (int friendId in friends)
            {
                InvokeCallback(friendId, (callback, id) => callback.OnFriendOnline(id), userId);
            }
        }

        private void NotifyFriendsUserOffline(int userId)
        {
            List<int> friends = GetFriendIds(userId);
            foreach (int friendId in friends)
            {



                InvokeCallback(friendId, (callback, id) => callback.OnFriendOffline(id), userId);
            }
        }


        public void InviteFriendToLobby(string inviterName, int userId, int lobbyId)
        {
            if (ActiveLobbiesDict.ContainsKey(lobbyId))
            {
                try
                {
                    if (!ConnectedClients.ContainsKey(userId))
                    {
                        Account account = new AccountDAO().GetAccountByUserId(userId);
                        if (account != null)
                        {
                            EmailUtils.SendInvitationByEmail(account.Email, lobbyId);
                        }
                    }
                    else
                    {
                        InvokeCallback(userId, cb => cb.NotifyGameInvited(inviterName, lobbyId));
                    }
                }
                catch (DataBaseException ex)
                {
                    throw CreateAndLogFaultException(ex);
                }
            }
        }


        private void ClientSocialChannelRuined(object sender, EventArgs e)
        {
            var callback = (ISocialManagerCallback)sender;
            RemoveClient(callback);
        }

        private void RemoveClient(ISocialManagerCallback callback)
        {
            KeyValuePair<int, ISocialManagerCallback> user = ConnectedClients.FirstOrDefault(pair => pair.Value == callback);
            if (user.Key != 0)
            {
                Disconnect(user.Key);
            }
        }

        public List<UserDTO> GetUsersFoundByName(int userId, string name)
        {
            var users = new List<UserDTO>();
            List<User> usersData = new UserDAO().GetUsersByName(name, userId);
            foreach (User user in usersData)
            {
                users.Add((UserDTO)user);
            }
            return users;
        }

        private List<int> GetFriendIds(int userId)
        {
            List<FriendDTO> friends = GetFriends(userId);
            return friends.Select(f => f.FriendId).ToList();
        }

    }
}