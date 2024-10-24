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
                //NEEDS TO SEND A MESSAGE TO THE CLIENT
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

            RemoveClientFromLobby(lobbyId, userId);

            connectedUsersDict.TryRemove(userId, out _);

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

        public void KickUser(int lobbyId, int userId)
        {
            throw new NotImplementedException();
        }

        private void ClientChannel_Closed(object sender, EventArgs e)
        {
            ILobbyManagerCallback callback = (ILobbyManagerCallback)sender;
            RemoveClient(callback);
            Console.WriteLine("Client Closed");
        }

        private void ClientChannel_Faulted(object sender, EventArgs e)
        {
            ILobbyManagerCallback callback = (ILobbyManagerCallback)sender;
            RemoveClient(callback);
            Console.WriteLine("Client faulted");
        }

        private void RemoveClient(ILobbyManagerCallback callback)
        {
            if (clientCallbackMapping.TryRemove(callback, out var clientInfo))
            {
                int lobbyId = clientInfo.LobbyId;
                int userId = clientInfo.UserId;

                RemoveClientFromLobby(lobbyId, userId);

                connectedUsersDict.TryRemove(userId, out _);
            }
        }

        private void RemoveClientFromLobby(int lobbyId, int userId)
        {
            if (activeLobbiesDict.TryGetValue(lobbyId, out var lobby))
            {
                lobby.TryRemove(userId, out var callback);

                clientCallbackMapping.TryRemove(callback, out _);
                Console.WriteLine(userId + " removed from lobby: " + lobbyId);
                foreach (var user in lobby)
                {
                    try
                    {
                        user.Value.OnLeaveLobby(lobbyId, userId);
                        
                    }
                    catch (Exception)
                    {
                        RemoveClient(user.Value);
                    }
                }
            }
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
        public void UpdateProfile(int userId, string username, int profilePictureId)
        {
            IProfileManagerCallback callback = OperationContext.Current.GetCallbackChannel<IProfileManagerCallback>();

            UserDAO userDAO = new UserDAO();
            User user = userDAO.GetUserById(userId);
            try{
                if (user != null)
                {
                    user.Username = username;
                    user.ProfilePictureId = profilePictureId;

                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate(username, profilePictureId, null);
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
