using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using BevososService.Utils;
using BevososService.DTOs; 
using DataAccess.DAO;
using DataAccess.Models;
using static BevososService.Utils.Hasher;
using BevososService.GameModels;
using System.Threading.Tasks;


namespace BevososService
{

    public partial class ServiceImplementation
    {
        public ServiceImplementation()
        {
            GlobalDeck.InitializeDeck();

            foreach (var item in GlobalDeck.Deck.Select(item => item.Value))
            {
                Console.WriteLine("Card " + item.CardId + " Element " + item.Element + " Type: " + item.Type + " Damage: " + item.Damage);
            }
        }


    }
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
            if (new TokenDAO().HasToken(email))
            {
                return EmailUtils.SendTokenByEmail(email, new TokenDAO().GetToken(email));
            }
            else
            {
                new TokenDAO().AsignToken(email);
                return EmailUtils.SendTokenByEmail(email, new TokenDAO().GetToken(email));
            }
        }

        public bool VerifyToken(string email, string token)
        {
            if (new TokenDAO().HasToken(email) && new TokenDAO().TokenIsValid(token, email))
            {
                new TokenDAO().DeleteToken(token, email);
                return true;
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

    public partial class ServiceImplementation : ILobbyManager
    {

        private static int _currentLobbyId = 4;

        // Lobby ID -> (User ID -> Callback)
        static ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>> activeLobbiesDict = new ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>>();

        // Callback -> (Lobby ID, User ID)
        static ConcurrentDictionary<ILobbyManagerCallback, (int LobbyId, int UserId)> clientCallbackMapping = new ConcurrentDictionary<ILobbyManagerCallback, (int LobbyId, int UserId)>();

        // User ID -> UserDto
        private static ConcurrentDictionary<int, UserDto> _lobbyUsersDetails = new ConcurrentDictionary<int, UserDto>();

        //  ID -> Lobby ID
        private static ConcurrentDictionary<int, int> _lobbyLeaders = new ConcurrentDictionary<int, int>();


        private static int GenerateUniqueLobbyId()
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

            _lobbyLeaders.TryAdd(lobbyId, userDto.UserId);

            clientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));

            userDto.IsReady = true;
            _lobbyUsersDetails.TryAdd(userDto.UserId, userDto);

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

            userDto.IsReady = true;
            _lobbyUsersDetails.TryAdd(userDto.UserId, userDto);

            var existingUsers = activeLobbiesDict[lobbyId]
                .Where(u => u.Key != userDto.UserId)
                .Select(u => _lobbyUsersDetails[u.Key])
                .ToList();

            callback.OnLobbyUsersUpdate(lobbyId, existingUsers);

            if (_lobbyLeaders.TryGetValue(lobbyId, out int leaderId))
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
            foreach (var user in activeLobbiesDict[lobbyId].Select(user => user.Value))
            {
                try
                {
                    user.OnSendMessage(userId, message);
                }
                catch (Exception)
                {
                    RemoveLobbyClient(user);
                }
            }
        }
        public void KickUser(int lobbyId, int kickerId, int targetUserId, string reason)
        {
            if (_lobbyLeaders.TryGetValue(lobbyId, out int leaderId) && leaderId == kickerId)
            {
                if (activeLobbiesDict.TryGetValue(lobbyId, out var lobby))
                {
                    if (lobby.TryGetValue(targetUserId, out var targetCallback))
                    {
                        try
                        {
                            targetCallback.OnKicked(lobbyId, reason);

                            RemoveClientFromLobby(lobbyId, targetUserId);
                            _lobbyUsersDetails.TryRemove(targetUserId, out _);
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
                if (_lobbyLeaders.TryGetValue(lobbyId, out int leaderId) && leaderId == userId)
                {
                    var remainingUsers = lobby.Keys.Where(k => k != userId).ToList();
                    if (remainingUsers.Any())
                    {
                        int newLeaderId = remainingUsers[0];
                        _lobbyLeaders.TryUpdate(lobbyId, newLeaderId, userId);

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
                        _lobbyLeaders.TryRemove(lobbyId, out _);
                    }
                }

                RemoveClientFromLobby(lobbyId, userId);
            }

            _lobbyUsersDetails.TryRemove(userId, out _);

        }
        public async void StartGame(int lobbyId)
        {
            if (activeLobbiesDict.TryGetValue(lobbyId, out var lobby))
            {
                int gameId = lobbyId;
                Game gameInstance = new Game
                {
                    GameId = gameId,
                    Players = new Dictionary<int, PlayerState>(),
                    Deck = new ConcurrentStack<Card>(),
                    BabyPiles = new Dictionary<int, Stack<Card>>(),
                    ActionsRemaining = 2 // This can be changed for each player if we have time
                };

                // Initialize the game
                InitializeGame(gameInstance, lobby);

                _activeGames.TryAdd(gameId, gameInstance);

                // Notify each player 
                foreach (var kvp in lobby.Select(x => x.Value))
                {
                    ILobbyManagerCallback lobbyCallback = kvp;
                    try
                    {
                        // Notify the player to join
                        lobbyCallback.GameStarted(gameId);
                    }
                    catch (Exception)
                    {
                        RemoveLobbyClient(lobbyCallback);
                    }
                }

                await Task.Delay(5000); // Wait for 5 seconds to remove the lobby
                activeLobbiesDict.TryRemove(lobbyId, out _);
            }
        }

        public void ChangeReadyStatus(int lobbyId,int userId)
        {
            if (_lobbyUsersDetails.TryGetValue(userId, out UserDto userDto))
            {
                userDto.IsReady = !userDto.IsReady;

                if(activeLobbiesDict.TryGetValue(lobbyId, out var lobby))
                {
                    foreach (var user in lobby.Values)
                    {
                        try
                        {
                            user.OnReadyStatusChanged(userId, userDto.IsReady);
                        }
                        catch (Exception)
                        {
                            RemoveLobbyClient(user);
                        }
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
                if (new UserDAO().UsernameExists(username))
                {
                    user.ProfilePictureId = profilePictureId;
                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate("Not changed", profilePictureId, "Username exists");
                    }
                }
                else if (username == "Not changed")
                {
                    user.ProfilePictureId = profilePictureId;
                    bool result = userDAO.UpdateUser(user);

                    if (result)
                    {
                        callback.OnProfileUpdate(username, profilePictureId, "");
                    }
                }
                else
                {

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

        public void Connect(int userId)
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
            if (new UserDAO().UserExists(userId) && new UserDAO().UserExists(blockedId))
            {
                return new BlockedDAO().DeleteBlock(userId, blockedId);
            }
            return false;
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

        public bool IsConnected(string email)
        {
            User user = new UserDAO().GetUserByEmail(email);
            if (user != null)
            {
                return connectedClients.ContainsKey(user.UserId);
            }
            return false;
        }
    }

    public partial class ServiceImplementation : IGameManager
    {
        //GameId-> GameInstance
        private static ConcurrentDictionary<int, Game> _activeGames = new ConcurrentDictionary<int, Game>();
        //GameId -> (UserId -> Callback)
        private static ConcurrentDictionary<int, ConcurrentDictionary<int, IGameManagerCallback>> _gamePlayerCallBack = new ConcurrentDictionary<int, ConcurrentDictionary<int, IGameManagerCallback>>();

        private void InitializeGame(Game gameInstance, ConcurrentDictionary<int, ILobbyManagerCallback> lobby)
        {
            // Initialize the deck

            List<Card> allCards = GlobalDeck.Deck.Values.ToList();

            // Shuffle the deck
            Shuffle(allCards);

            // Assign the shuffled deck to the game instance
            gameInstance.Deck = new ConcurrentStack<Card>(allCards);

            // Initialize BabyPiles
            gameInstance.BabyPiles = new Dictionary<int, Stack<Card>>
            {
                { 0, new Stack<Card>() }, // Baby of Land
                { 1, new Stack<Card>() }, // Baby of Water
                { 2, new Stack<Card>() }  // Baby of Air
            };

            // 3. Initialize Players
            foreach (var userId in lobby.Select(x => x.Key))
            {
                UserDto userDto = _lobbyUsersDetails[userId];

                PlayerState playerState = new PlayerState
                {
                    User = userDto,
                    Hand = new List<Card>(),
                    Monsters = new List<Monster>()
                };


                // Draw initial hand (e.g., 5 cards per player)
                for (int i = 0; i < 5; i++)
                {
                    if (gameInstance.Deck.TryPop(out Card card))
                    {
                        playerState.Hand.Add(card);
                    }
                }

                gameInstance.Players.Add(userId, playerState);
            }

            // 4. Set the Current Player
            gameInstance.CurrentPlayerId = gameInstance.Players.Keys.First();

            gameInstance.ActionsRemaining = 2;
        }

        public void JoinGame(int gameId, UserDto userDto)
        {
            IGameManagerCallback callback = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();
            ICommunicationObject clientChannel = (ICommunicationObject)callback;

            clientChannel.Closed += GameChannel_Closed;
            clientChannel.Faulted += GameChannel_Closed;

            if (_activeGames.TryGetValue(gameId, out Game gameInstance))
            {
                if (!_gamePlayerCallBack.ContainsKey(gameId))
                {
                    _gamePlayerCallBack.TryAdd(gameId, new ConcurrentDictionary<int, IGameManagerCallback>());
                }

                _gamePlayerCallBack[gameId].TryAdd(userDto.UserId, callback);

                if (gameInstance.Players.TryGetValue(userDto.UserId, out PlayerState playerState))
                {
                    GameStateDTO gameStateDto = (GameStateDTO)gameInstance;

                    callback.ReceiveGameState(gameStateDto);
                }
                else
                {
                    throw new FaultException("Player not found in game.");
                }
            }
            else
            {
                throw new FaultException("Game does not exist.");
            }
        }

        private static void GameChannel_Closed(object sender, EventArgs e)
        {
            var callback = (IGameManagerCallback)sender;
            RemoveGameClient(callback);
        }

        private static void RemoveGameClient(IGameManagerCallback callback)
        {
            foreach (var gameEntry in _gamePlayerCallBack)
            {
                int gameId = gameEntry.Key;
                var playerCallbacks = gameEntry.Value;

                var playerToRemove = playerCallbacks.FirstOrDefault(kvp => kvp.Value == callback);
                if (!playerToRemove.Equals(default(KeyValuePair<int, IGameManagerCallback>)))
                {
                    int userId = playerToRemove.Key;
                    playerCallbacks.TryRemove(userId, out _);

                    Console.WriteLine($"Player {userId} removed from game {gameId}");

                    if (playerCallbacks.Count < 2)
                    {
                        Console.WriteLine($"Game {gameId} no longer has enough players to continue. Ending game.");
                        EndGame(gameId);
                    }

                    if (_activeGames.TryGetValue(gameId, out Game gameInstance))
                    {
                        gameInstance.Players.Remove(userId);
                    }

                    return;
                }
            }

            Console.WriteLine("Callback not found in any active game.");
        }

        private static void EndGame(int gameId)
        {
            if (_activeGames.TryRemove(gameId, out _))
            {
                _gamePlayerCallBack.TryRemove(gameId, out _);
                Console.WriteLine($"Game {gameId} has been ended and removed from active games.");
            }
        }

        public void DrawCard(int matchCode, int userId)
        {
            _activeGames.TryGetValue(matchCode, out Game gameInstance);
            gameInstance.Deck.TryPop(out Card card);
            gameInstance.Players[userId].Hand.Add(card);


            for (int i = 0; i < gameInstance.Players.Count; i++)
            {
                Console.WriteLine(card.CardId);
                _gamePlayerCallBack[matchCode][gameInstance.Players.ElementAt(i).Key].ReceiveGameState((GameStateDTO)gameInstance);
            }
        }

        public void PlayCard(int userId, int matchCode, int cardId)
        {
            Card card = GlobalDeck.Deck[cardId];

            if (card.Type == Card.CardType.Baby)
            {
                PlayBaby(userId, matchCode, card);
            }
            if (card.Type == Card.CardType.Head)
            {
                /*
                 * This will use a callback to notify the player that that 
                 * the card was played successfully
                 * or if his monsters are already full
                 */
                PlayHead(userId, matchCode, card);
            }
            if (card.Type == Card.CardType.WildProvoke)
            {
                /*
                 * This will use a callback to ask the player to select a deck to provoke
                 * Enforcing this in the client side will be tuff
                 * 
                 */
                //PlayProvoke(userId, matchCode, card);
            }
            if (card.Type == Card.CardType.BodyPart)
            {
                /*
                 * This will call a callback to ask the player to select a monster to attach the body part to
                 * 
                 */
                //PlayBodyPart(userId, matchCode, card);
            }
            if (card.Type == Card.CardType.Tool)
            {
                /*
                 * 
                 * This will call a callback to ask the player to select a monster to attach the tool to
                 * 
                 */
                //PlayTool(userId, matchCode, card);
            }
            if (card.Type == Card.CardType.Hat)
            {
                /*
                 * This will call a callback to ask the player to select a monster to attach the hat to
                 * 
                 */
                //PlayHat(userId, matchCode, card);
            }

        }

        private void PlayBaby(int userId, int matchCode, Card card)
        {
            switch (card.Element)
            {
                case Card.CardElement.Land:
                    _activeGames[matchCode].BabyPiles[0].Push(card);
                    _activeGames[matchCode].Players[userId].Hand.Remove(card);
                    break;
                case Card.CardElement.Water:
                    _activeGames[matchCode].BabyPiles[1].Push(card);
                    _activeGames[matchCode].Players[userId].Hand.Remove(card);
                    break;
                case Card.CardElement.Air:
                    _activeGames[matchCode].BabyPiles[2].Push(card);
                    _activeGames[matchCode].Players[userId].Hand.Remove(card);
                    break;
            }

            _activeGames.TryGetValue(matchCode, out Game gameInstance);

            for (int i = 0; i < gameInstance.Players.Count; i++)
            {
                Console.WriteLine(card.CardId);
                _gamePlayerCallBack[matchCode][gameInstance.Players.ElementAt(i).Key].ReceiveGameState((GameStateDTO)gameInstance);
            }
        }


        private void PlayHead(int userId, int matchCode, Card card)
        {
            if (_activeGames.TryGetValue(matchCode, out Game gameInstance))
            {
                if (gameInstance.Players[userId].Monsters.Count < 3)
                {
                    Monster monster = new Monster
                    {
                        Head = card,
                        Torso = null,
                        LeftHand = null,
                        LeftHandTool = null,
                        RightHand = null,
                        RightHandTool = null,
                        Legs = null,
                        Hat = null
                    };
                    gameInstance.Players[userId].Monsters.Add(monster);
                    gameInstance.Players[userId].Hand.Remove(card);

                    for (int i = 0; i < gameInstance.Players.Count; i++)
                    {
                        Console.WriteLine(card.CardId);
                        _gamePlayerCallBack[matchCode][gameInstance.Players.ElementAt(i).Key].ReceiveGameState((GameStateDTO)gameInstance);
                    }
                }
            }

        }

        private void PlayBodyPart(int userId, int matchCode, int MonsterChosenIndex, Card card)
        {
            if (_activeGames.TryGetValue(matchCode, out Game gameInstance))
            {

            }

        }
    }

    public partial class ServiceImplementation : ICardManager
    {
        public static void SeeGlobalDeck()
        {
            foreach (var card in GlobalDeck.Deck)
            {
                Console.WriteLine(card.Value);
            }
        }

        private static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }

    public partial class ServiceImplementation : IPlayerManager
    {

    }

    }

