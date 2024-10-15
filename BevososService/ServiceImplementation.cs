using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
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

        public void SendToken(string email)
        {
            if(new TokenDAO().HasToken(email))
            {
               EmailUtils.SendTokenByEmail(email, new TokenDAO().GetToken(email));
            }else
            {
                new TokenDAO().AsignToken(email);
                EmailUtils.SendTokenByEmail(email, new TokenDAO().GetToken(email));
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


    }



    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]

    public partial class ServiceImplementation: ILobbyManager
    {

        private static int _currentLobbyId = 4;
        static ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>> activeLobbiesDict = new ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>>();


        private int GenerateUniqueLobbyId()
        {
            return Interlocked.Increment(ref _currentLobbyId);
        }


        public void NewLobbyCreated(int userId)
        {

            int lobbyId = GenerateUniqueLobbyId();


            ILobbyManagerCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
            if (!activeLobbiesDict.ContainsKey(lobbyId))
            {
                activeLobbiesDict.TryAdd(lobbyId, new ConcurrentDictionary<int, ILobbyManagerCallback>());
                
            }

            activeLobbiesDict[lobbyId].TryAdd(userId, callback);
            callback.OnNewLobbyCreated(lobbyId, userId);  
        }

        public void JoinLobby(int lobbyId, UserDto userDto)
        {
            ILobbyManagerCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
            if (!activeLobbiesDict.ContainsKey(lobbyId))
            {
                activeLobbiesDict.TryAdd(lobbyId, new ConcurrentDictionary<int, ILobbyManagerCallback>());
            }
            activeLobbiesDict[lobbyId].TryAdd(userDto.UserId, callback);

            foreach (var user in activeLobbiesDict[lobbyId])
            {
                user.Value.OnJoinLobby(lobbyId, userDto);
            }
        }

        public void LeaveLobby(int lobbyId, int userId)
        {
            activeLobbiesDict[lobbyId].TryRemove(userId, out ILobbyManagerCallback callback);
            foreach (var user in activeLobbiesDict[lobbyId])
            {
                user.Value.OnLeaveLobby(lobbyId, userId);
            }
        }

        public void SendMessage(int lobbyId, int userId, string message)
        {
            foreach (var user in activeLobbiesDict[lobbyId])
            {
                user.Value.OnSendMessage(userId, message);
            }
        }

        public void KickUser(int lobbyId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
