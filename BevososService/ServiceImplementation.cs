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
        private static ConcurrentDictionary<int, UserDto> connectedUsersDict = new ConcurrentDictionary<int, UserDto>();

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

            clientChannel.Closed += ClientChannel_Closed;
            clientChannel.Faulted += ClientChannel_Faulted;

            activeLobbiesDict.TryAdd(lobbyId, new ConcurrentDictionary<int, ILobbyManagerCallback>());
            activeLobbiesDict[lobbyId].TryAdd(userDto.UserId, callback);

            lobbyLeaders.TryAdd(lobbyId, userDto.UserId);

            clientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));
            connectedUsersDict.TryAdd(userDto.UserId, userDto);

            callback.OnNewLobbyCreated(lobbyId, userDto.UserId);
        }
        public void JoinLobby(int lobbyId, UserDto userDto)
        {
            ILobbyManagerCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
            ICommunicationObject clientChannel = (ICommunicationObject)callback;

            clientChannel.Closed += ClientChannel_Closed;
            clientChannel.Faulted += ClientChannel_Faulted;

            if (!activeLobbiesDict.ContainsKey(lobbyId))
            {
                return;
            }

            activeLobbiesDict[lobbyId].TryAdd(userDto.UserId, callback);
            clientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));

            connectedUsersDict.TryAdd(userDto.UserId, userDto);

            var existingUsers = activeLobbiesDict[lobbyId]
                .Where(u => u.Key != userDto.UserId)
                .Select(u => connectedUsersDict[u.Key])
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
                        RemoveClient(user.Value);
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
                    RemoveClient(user.Value);
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
                            connectedUsersDict.TryRemove(targetUserId, out _);
                        }
                        catch (Exception)
                        {
                            RemoveClient(targetCallback);
                        }
                    }
                }
            }
        }

        private void ClientChannel_Closed(object sender, EventArgs e)
        {
            var callback = (ILobbyManagerCallback)sender;
            RemoveClient(callback);
            Console.WriteLine("Client Closed");
        }

        private void ClientChannel_Faulted(object sender, EventArgs e)
        {
            var callback = (ILobbyManagerCallback)sender;
            RemoveClient(callback);
            Console.WriteLine("Client Faulted");
        }
        private void RemoveClient(ILobbyManagerCallback callback)
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
                            RemoveClient(user);
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
                                RemoveClient(user);
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

            connectedUsersDict.TryRemove(userId, out _);
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
}
