using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using BevososService.Utils;
using BevososService.DTOs; 
using DataAccess.DAO;
using DataAccess.Models;
using static BevososService.Utils.Hasher;
using System.Collections.ObjectModel;

namespace BevososService
{
    public partial class ServiceImplementation : IUsersManager
    {
        public bool IsEmailTaken(string email)
        {
             return new AccountDAO().EmailExists(email);
        }

        public bool IsUsernameTaken(string username)
        {
            return new UserDAO().UsernameExists(username);
        }

        public bool RegisterUser(string email, string username, string password)
        {
            User user = new User();
            user.Username = username;

            Account account = new Account();
            account.Email = email;
            account.PasswordHash = SimpleHashing.HashPassword(password);

            return new AccountDAO().AddUserWithAccount(user, account);
        }

        public bool SendToken(string email)
        {
            if(new TokenDAO().HasToken(email))
            {
                return EmailUtils.SendTokenByEmail(email, new TokenDAO().GetToken(email));
            }else
            {
                new TokenDAO().AsignToken(email);
                return EmailUtils.SendTokenByEmail(email, new TokenDAO().GetToken(email));
            }
        }

        public bool VerifyToken(string email, string token)
        {
            if (new TokenDAO().HasToken(email))
            {
                if(new TokenDAO().TokenIsValid(token, email))
                {
                    new TokenDAO().DeleteToken(token, email);
                    return true;
                }
            }
            return false;
        }
        public UserDto LogIn(string email, string password)
        {
            AccountDAO accountDAO = new AccountDAO();
            UserDAO userDAO = new UserDAO();


            Account account = accountDAO.GetAccountByEmail(email);

            if (account == null)
            {
                return null;
            }

                if (SimpleHashing.VerifyPassword(password, account.PasswordHash))
                {
                    User user = userDAO.GetUserById(account.UserId);

                    UserDto userDto = new UserDto
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = account.Email,
                        ProfilePictureId = user.ProfilePictureId
                    };

                    return userDto;
                }
            return null;
        }

        public bool RecoverPassword(string email, string password)
        {
            string hashedPassword = SimpleHashing.HashPassword(password);
            AccountDAO accountDAO = new AccountDAO();
            return accountDAO.UpdatePasswordByEmail(email, hashedPassword);
        }

    }



    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]

    public partial class ServiceImplementation: ILobbyManager
    {

        private static int _currentLobbyId = 4;

        // Lobby ID -> (User ID -> Callback)
        static ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>> activeLobbiesDict = new ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>>();

        // Callback -> (Lobby ID, User ID)
        static ConcurrentDictionary<ILobbyManagerCallback, (int LobbyId, int UserId)> clientCallbackMapping = new ConcurrentDictionary<ILobbyManagerCallback, (int LobbyId, int UserId)>();

        // User ID -> UserDto
        private static ConcurrentDictionary<int, UserDto> lobbyUsersDetails = new ConcurrentDictionary<int, UserDto>();

        //  ID -> Lobby ID
        private static ConcurrentDictionary<int, int> lobbyLeaders = new ConcurrentDictionary<int, int>();


        private int GenerateUniqueLobbyId()
        {
            return Interlocked.Increment(ref _currentLobbyId);
        }

        public void NewLobbyCreated(UserDto userDto)
        {
            int lobbyId = GenerateUniqueLobbyId();

            ILobbyManagerCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
            ICommunicationObject clientChannel = (ICommunicationObject)callback;

            clientChannel.Closed += LobbyChannel_Closed;
            clientChannel.Faulted += LobbyChannel_Faulted;

            activeLobbiesDict.TryAdd(lobbyId, new ConcurrentDictionary<int, ILobbyManagerCallback>());
            activeLobbiesDict[lobbyId].TryAdd(userDto.UserId, callback);

            lobbyLeaders.TryAdd(lobbyId, userDto.UserId);

            clientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));
            lobbyUsersDetails.TryAdd(userDto.UserId, userDto);

            callback.OnNewLobbyCreated(lobbyId, userDto.UserId);
        }
        public void JoinLobby(int lobbyId, UserDto userDto)
        {
            ILobbyManagerCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
            ICommunicationObject clientChannel = (ICommunicationObject)callback;

            clientChannel.Closed += LobbyChannel_Closed;
            clientChannel.Faulted += LobbyChannel_Faulted;

            if (!activeLobbiesDict.ContainsKey(lobbyId))
            {
                return;
            }

            activeLobbiesDict[lobbyId].TryAdd(userDto.UserId, callback);
            clientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));

            lobbyUsersDetails.TryAdd(userDto.UserId, userDto);

            var existingUsers = activeLobbiesDict[lobbyId]
                .Where(u => u.Key != userDto.UserId)
                .Select(u => lobbyUsersDetails[u.Key])
                .ToList();

            callback.OnLobbyUsersUpdate(lobbyId, existingUsers);

            if (lobbyLeaders.TryGetValue(lobbyId, out int leaderId))
            {
                callback.OnLeaderChanged(lobbyId, leaderId);
            }

            foreach (var user in activeLobbiesDict[lobbyId])
            {
                if (user.Key != userDto.UserId)
                {
                    try
                    {
                        user.Value.OnJoinLobby(lobbyId, userDto);
                    }
                    catch (Exception)
                    {
                        RemoveLobbyClient(user.Value);
                    }
                }
            }
        
        }
        public void LeaveLobby(int lobbyId, int userId)
        {
            HandleUserLeavingLobby(lobbyId, userId);
        }

        public void SendMessage(int lobbyId, int userId, string message)
        {
            foreach (var user in activeLobbiesDict[lobbyId])
            {
                try { 
                    user.Value.OnSendMessage(userId, message);
                }catch(Exception)
                {
                    RemoveLobbyClient(user.Value);
                }
            }
        }

        public void KickUser(int lobbyId, int kickerId, int targetUserId, string reason)
        {
            if (lobbyLeaders.TryGetValue(lobbyId, out int leaderId) && leaderId == kickerId)
            {
                if (activeLobbiesDict.TryGetValue(lobbyId, out var lobby))
                {
                    if (lobby.TryGetValue(targetUserId, out var targetCallback))
                    {
                        try
                        {
                            targetCallback.OnKicked(lobbyId, reason);

                            RemoveClientFromLobby(lobbyId, targetUserId);
                            lobbyUsersDetails.TryRemove(targetUserId, out _);
                        }
                        catch (Exception)
                        {
                            RemoveLobbyClient(targetCallback);
                        }
                    }
                }
            }
        }

        private void LobbyChannel_Closed(object sender, EventArgs e)
        {
            var callback = (ILobbyManagerCallback)sender;
            RemoveLobbyClient(callback);
            Console.WriteLine("Client Closed");
        }

        private void LobbyChannel_Faulted(object sender, EventArgs e)
        {
            var callback = (ILobbyManagerCallback)sender;
            RemoveLobbyClient(callback);
            Console.WriteLine("Client Faulted");
        }
        private void RemoveLobbyClient(ILobbyManagerCallback callback)
        {
            if (clientCallbackMapping.TryRemove(callback, out var clientInfo))
            {
                int lobbyId = clientInfo.LobbyId;
                int userId = clientInfo.UserId;

                HandleUserLeavingLobby(lobbyId, userId);
            }

        }

        private void RemoveClientFromLobby(int lobbyId, int userId)
        {
            if (activeLobbiesDict.TryGetValue(lobbyId, out var lobby))
            {
                if (lobby.TryRemove(userId, out var callback))
                {
                    Console.WriteLine($"{userId} removed from lobby: {lobbyId}");

                    foreach (var user in lobby.Values)
                    {
                        try
                        {
                            user.OnLeaveLobby(lobbyId, userId);
                        }
                        catch (Exception)
                        {
                            RemoveLobbyClient(user);
                        }
                    }
                }
                clientCallbackMapping.TryRemove(callback, out _);
            }
        }

        private void HandleUserLeavingLobby(int lobbyId, int userId)
        {
            if (activeLobbiesDict.TryGetValue(lobbyId, out var lobby))
            {
                if (lobbyLeaders.TryGetValue(lobbyId, out int leaderId) && leaderId == userId)
                {
                    var remainingUsers = lobby.Keys.Where(k => k != userId).ToList();
                    if (remainingUsers.Any())
                    {
                        int newLeaderId = remainingUsers.First();
                        lobbyLeaders.TryUpdate(lobbyId, newLeaderId, userId);

                        foreach (var user in lobby.Values)
                        {
                            try
                            {
                                user.OnLeaderChanged(lobbyId, newLeaderId);
                            }
                            catch (Exception)
                            {
                                RemoveLobbyClient(user);
                            }
                        }
                    }
                    else
                    {
                        activeLobbiesDict.TryRemove(lobbyId, out _);
                        lobbyLeaders.TryRemove(lobbyId, out _);
                    }
                }

                RemoveClientFromLobby(lobbyId, userId);
            }

            lobbyUsersDetails.TryRemove(userId, out _);
        }
    }

    public partial class ServiceImplementation : ILobbyChecker
    {
        public bool IsLobbyOpen(int lobbyId)
        {
            return activeLobbiesDict.ContainsKey(lobbyId);
        }

        public bool IsLobbyFull(int lobbyId)
        {
            if (activeLobbiesDict.TryGetValue(lobbyId, out var lobby))
            {
                return lobby.Count >= 4;
            }
            return false;
        }
    }

    public partial class ServiceImplementation : IProfileManager
    {
        public void ChangePassword(int userId, string oldPassword, string newPassword)
        {
            IProfileManagerCallback callback = OperationContext.Current.GetCallbackChannel<IProfileManagerCallback>();

            AccountDAO accountDAO = new AccountDAO();
            Account account = accountDAO.GetAccountByUserId(userId);

            if (SimpleHashing.VerifyPassword(oldPassword, account.PasswordHash))
            {
                string newHashedPassword = SimpleHashing.HashPassword(newPassword);
                bool result = accountDAO.UpdatePasswordByUserId(userId, newHashedPassword);

                if (result)
                {
                    callback.OnPasswordChange(null);
                }
                else
                {
                    callback.OnPasswordChange("Failed to update password.");
                }
            }
            else
            {
                callback.OnPasswordChange("Incorrect password.");
            }

        }

        public void UpdateProfile(int userId, string username, int profilePictureId)
        {
            IProfileManagerCallback callback = OperationContext.Current.GetCallbackChannel<IProfileManagerCallback>();

            UserDAO userDAO = new UserDAO();
            User user = userDAO.GetUserById(userId);

            try
            {
                if (new UserDAO().UsernameExists(username) == true)
                {
                    user.ProfilePictureId = profilePictureId;
                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate( "Not changed", profilePictureId, "Username exists");
                    }
                }
                else if(username == "Not changed"){
                    user.ProfilePictureId = profilePictureId;
                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate(username, profilePictureId, "");
                    }
                }
                else {
                    
                    user.Username = username;
                    user.ProfilePictureId = profilePictureId;

                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate(username, profilePictureId, "");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }


    public partial class ServiceImplementation : ISocialManager
    {

        private static readonly ConcurrentDictionary<int, ISocialManagerCallback> connectedClients = new ConcurrentDictionary<int, ISocialManagerCallback>();
        
        private readonly object _lock = new object();

        public void Connect(int userid)
        {
            var callback = OperationContext.Current.GetCallbackChannel<ISocialManagerCallback>();
            ICommunicationObject clientChannel = (ICommunicationObject)callback;

            Console.WriteLine("Client connected: " + userid);

            clientChannel.Closed += ClientChannel_Closed;
            clientChannel.Faulted += ClientChannel_Faulted;

            lock (_lock)
            {
                connectedClients.TryAdd(userid, callback);
            }
        
            NotifyFriendsUserOnline(userid);
        }

        public void Disconnect(int userid)
        {
            lock (_lock)
            {
                connectedClients.TryRemove(userid, out _);
            }

            NotifyFriendsUserOffline(userid);
        }

        private void ClientChannel_Closed(object sender, EventArgs e)
        {
            var callback = (ISocialManagerCallback)sender;
            RemoveClient(callback);
        }

        private void ClientChannel_Faulted(object sender, EventArgs e)
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
            if(new UserDAO().UserExists(userId) && new UserDAO().UserExists(friendId))
            {
                if(connectedClients.TryGetValue(friendId, out var friendCallback))
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
                    if(connectedClients.TryGetValue(friendId, out var friendCallback))
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
            if(new UserDAO().UserExists(userId))
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

        public List<FriendRequestDTO> GetFriendRequests(int userId)
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

        public List<FriendDTO> GetFriends(int userId)
        {
            if (new UserDAO().UserExists(userId))
            {
                List<FriendData> friendshipList =new FriendshipDAO().GetFriendsForUser(userId);
                List<FriendDTO> friends = new List<FriendDTO>();
                foreach (FriendData friend in friendshipList)
                {
                    if (connectedClients.TryGetValue(friend.FriendId, out var friendCallback))
                    {
                        friend.IsConnected = true;
                    }

                    friends.Add((FriendDTO) friend);
                }
                return friends;
            }
            return null;
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

        public void AcceptFriendRequest(int userId, int friendId, int requestId)
        {
            if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(friendId))
            {
                bool result = new FriendRequestDAO().AcceptFriendRequest(requestId);
                Console.WriteLine("resultado añadir: " + result);
                if (result)
                {
                    Friendship friendship = new FriendshipDAO().AddFriendship(userId, friendId);
                    if (friendship != null)
                    {
                        int friendshipId = friendship.Id;
                        Console.WriteLine("Friendship ID: " + friendshipId);

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
                            userCallback.OnNewFriend(friendDto);
                            Console.WriteLine("Amigo añadido1");
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
                            friendCallback.OnNewFriend(friendDtoForFriend);
                            Console.WriteLine("Amigo añadido22222");
                        }

                        Console.WriteLine("Todo Jalo");
                        return;
                    }
                }
            }
            Console.WriteLine("Nadota Jalo");
            return;
        }
        public bool DeclineFriendRequest(int requestId)
        {
            if (new FriendRequestDAO().FriendRequestExists(requestId))
            {
                return new FriendRequestDAO().DeclineFriendRequest(requestId);
            }
            return false;
        }
        public bool UnblockUser(int userId, int blockedId)
        {
            if(new UserDAO().UserExists(userId) && new UserDAO().UserExists(blockedId))
            {
                return new BlockedDAO().DeleteBlock(userId, blockedId);
            }
            return false;
        }

        public List<UserDto> GetUsersFoundByName(int userId,string name)
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
            if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(blockeeId))
            {
                if(new BlockedDAO().AddBlock(userId, blockeeId))
                { 
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool IsConnected(string email)
        {
            User user= new UserDAO().GetUserByEmail(email);
            if (user != null) 
            {
                return connectedClients.ContainsKey(user.UserId);
            }
            return false;
        }
    }
}
