using BevososService.DTOs;
using BevososService.GameModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BevososService.Implementations
{

    //NEEDS STEROID CALLBACK EXCEPTION HANDLING

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]

    public partial class ServiceImplementation : ILobbyManager
    {

        private static int _currentLobbyId = 4;

        // Lobby ID -> (User ID -> Callback)
        static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>> _activeLobbiesDict = new ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>>();

        // Callback -> (Lobby ID, User ID)
        static readonly ConcurrentDictionary<ILobbyManagerCallback, (int LobbyId, int UserId)> _clientCallbackMapping = new ConcurrentDictionary<ILobbyManagerCallback, (int LobbyId, int UserId)>();

        // User ID -> UserDto
        private static readonly ConcurrentDictionary<int, UserDto> _lobbyUsersDetails = new ConcurrentDictionary<int, UserDto>();

        //  ID -> Lobby ID
        private static readonly ConcurrentDictionary<int, int> _lobbyLeaders = new ConcurrentDictionary<int, int>();


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

            _activeLobbiesDict.TryAdd(lobbyId, new ConcurrentDictionary<int, ILobbyManagerCallback>());
            _activeLobbiesDict[lobbyId].TryAdd(userDto.UserId, callback);

            _lobbyLeaders.TryAdd(lobbyId, userDto.UserId);

            _clientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));

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

            if (!_activeLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> value))
            {
                return;
            }

            value.TryAdd(userDto.UserId, callback);
            _clientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));

            userDto.IsReady = true;
            _lobbyUsersDetails.TryAdd(userDto.UserId, userDto);

            List<UserDto> existingUsers = _activeLobbiesDict[lobbyId]
                .Where(u => u.Key != userDto.UserId)
                .Select(u => _lobbyUsersDetails[u.Key])
                .ToList();

            callback.OnLobbyUsersUpdate(lobbyId, existingUsers);

            if (_lobbyLeaders.TryGetValue(lobbyId, out int leaderId))
            {
                callback.OnLeaderChanged(lobbyId, leaderId);
            }

            foreach (KeyValuePair<int, ILobbyManagerCallback> user in _activeLobbiesDict[lobbyId])
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
            foreach (ILobbyManagerCallback user in _activeLobbiesDict[lobbyId].Select(user => user.Value))
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
                if (_activeLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
                {
                    if (lobby.TryGetValue(targetUserId, out ILobbyManagerCallback targetCallback))
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
            ILobbyManagerCallback callback = (ILobbyManagerCallback)sender;
            RemoveLobbyClient(callback);
            Console.WriteLine("Client Closed");
        }
        private void LobbyChannel_Faulted(object sender, EventArgs e)
        {
            ILobbyManagerCallback callback = (ILobbyManagerCallback)sender;
            RemoveLobbyClient(callback);
            Console.WriteLine("Client Faulted");
        }
        private void RemoveLobbyClient(ILobbyManagerCallback callback)
        {
            if (_clientCallbackMapping.TryRemove(callback, out (int LobbyId, int UserId) clientInfo))
            {
                int lobbyId = clientInfo.LobbyId;
                int userId = clientInfo.UserId;

                HandleUserLeavingLobby(lobbyId, userId);
            }

        }
        private void RemoveClientFromLobby(int lobbyId, int userId)
        {
            if (_activeLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
            {
                if (lobby.TryRemove(userId, out ILobbyManagerCallback callback))
                {
                    Console.WriteLine($"{userId} removed from lobby: {lobbyId}");

                    foreach (ILobbyManagerCallback user in lobby.Values)
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
                _clientCallbackMapping.TryRemove(callback, out _);
            }
        }
        private void HandleUserLeavingLobby(int lobbyId, int userId)
        {
            if (_activeLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
            {
                if (_lobbyLeaders.TryGetValue(lobbyId, out int leaderId) && leaderId == userId)
                {
                    List<int> remainingUsers = lobby.Keys.Where(k => k != userId).ToList();
                    if (remainingUsers.Any())
                    {
                        int newLeaderId = remainingUsers[0];
                        _lobbyLeaders.TryUpdate(lobbyId, newLeaderId, userId);

                        foreach (ILobbyManagerCallback user in lobby.Values)
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
                        _activeLobbiesDict.TryRemove(lobbyId, out _);
                        _lobbyLeaders.TryRemove(lobbyId, out _);
                    }
                }

                RemoveClientFromLobby(lobbyId, userId);
            }

            _lobbyUsersDetails.TryRemove(userId, out _);

        }
        public async void StartGame(int lobbyId)
        {
            if (_activeLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
            {
                int gameId = lobbyId;
                Game gameInstance = new Game
                {
                    GameId = gameId,
                    Players = new Dictionary<int, PlayerState>(),
                    Deck = new ConcurrentStack<Card>(),
                    BabyPiles = new Dictionary<int, Stack<Card>>(),
                    ActionsPerTurn = 2 // This can be changed for each player if we have time
                };

                // Initialize the game


                _activeGames.TryAdd(gameId, gameInstance);

                InitializeGame(gameInstance, lobby);

                // Notify each player 
                foreach (ILobbyManagerCallback kvp in lobby.Select(x => x.Value))
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
                _activeLobbiesDict.TryRemove(lobbyId, out _);
            }
        }
        public void ChangeReadyStatus(int lobbyId, int userId)
        {
            if (_lobbyUsersDetails.TryGetValue(userId, out UserDto userDto))
            {
                userDto.IsReady = !userDto.IsReady;

                if (_activeLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
                {
                    foreach (ILobbyManagerCallback user in lobby.Values)
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
            return _activeLobbiesDict.ContainsKey(lobbyId);
        }

        public bool IsLobbyFull(int lobbyId)
        {
            if (_activeLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
            {
                return lobby.Count >= 4;
            }
            return false;
        }

    }
}
