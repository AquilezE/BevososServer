using BevososService.DTOs;
using BevososService.GameModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
        static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>> ActiveLobbiesDict = new ConcurrentDictionary<int, ConcurrentDictionary<int, ILobbyManagerCallback>>();

        // Callback -> (Lobby ID, User ID)
        static readonly ConcurrentDictionary<ILobbyManagerCallback, (int LobbyId, int UserId)> ClientCallbackMapping = new ConcurrentDictionary<ILobbyManagerCallback, (int LobbyId, int UserId)>();

        // User ID -> UserDTO
        private static readonly ConcurrentDictionary<int, UserDTO> LobbyUsersDetails = new ConcurrentDictionary<int, UserDTO>();

        //  ID -> Lobby ID
        private static readonly ConcurrentDictionary<int, int> LobbyLeaders = new ConcurrentDictionary<int, int>();


        private static int GenerateUniqueLobbyId()
        {
            return Interlocked.Increment(ref _currentLobbyId);
        }

        public void NewLobbyCreated(UserDTO userDto)
        {
            var lobbyId = GenerateUniqueLobbyId();

            var callback = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
            var clientChannel = (ICommunicationObject)callback;

            clientChannel.Closed += LobbyChannel_Closed;
            clientChannel.Faulted += LobbyChannel_Faulted;

            ActiveLobbiesDict.TryAdd(lobbyId, new ConcurrentDictionary<int, ILobbyManagerCallback>());
            ActiveLobbiesDict[lobbyId].TryAdd(userDto.UserId, callback);

            LobbyLeaders.TryAdd(lobbyId, userDto.UserId);

            ClientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));

            userDto.IsReady = true;
            LobbyUsersDetails.TryAdd(userDto.UserId, userDto);

            callback.OnNewLobbyCreated(lobbyId, userDto.UserId);
        }
        public void JoinLobby(int lobbyId, UserDTO userDto)
        {
            var callback = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
            var clientChannel = (ICommunicationObject)callback;

            clientChannel.Closed += LobbyChannel_Closed;
            clientChannel.Faulted += LobbyChannel_Faulted;

            if (!ActiveLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> value))
            {
                return;
            }

            value.TryAdd(userDto.UserId, callback);
            ClientCallbackMapping.TryAdd(callback, (lobbyId, userDto.UserId));

            userDto.IsReady = true;
            LobbyUsersDetails.TryAdd(userDto.UserId, userDto);

            List<UserDTO> existingUsers = ActiveLobbiesDict[lobbyId]
                .Where(u => u.Key != userDto.UserId)
                .Select(u => LobbyUsersDetails[u.Key])
                .ToList();

            callback.OnLobbyUsersUpdate(lobbyId, existingUsers);

            if (LobbyLeaders.TryGetValue(lobbyId, out var leaderId))
            {
                callback.OnLeaderChanged(lobbyId, leaderId);
            }

            foreach (KeyValuePair<int, ILobbyManagerCallback> user in ActiveLobbiesDict[lobbyId])
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
            foreach (ILobbyManagerCallback user in ActiveLobbiesDict[lobbyId].Select(user => user.Value))
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
            if (LobbyLeaders.TryGetValue(lobbyId, out var leaderId) && leaderId == kickerId)
            {
                if (ActiveLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
                {
                    if (lobby.TryGetValue(targetUserId, out ILobbyManagerCallback targetCallback))
                    {
                        try
                        {
                            targetCallback.OnKicked(lobbyId, reason);

                            RemoveClientFromLobby(lobbyId, targetUserId);
                            LobbyUsersDetails.TryRemove(targetUserId, out _);
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
            if (ClientCallbackMapping.TryRemove(callback, out (int LobbyId, int UserId) clientInfo))
            {
                var lobbyId = clientInfo.LobbyId;
                var userId = clientInfo.UserId;

                HandleUserLeavingLobby(lobbyId, userId);
            }

        }
        private void RemoveClientFromLobby(int lobbyId, int userId)
        {
            if (ActiveLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
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
                ClientCallbackMapping.TryRemove(callback, out _);
            }
        }
        private void HandleUserLeavingLobby(int lobbyId, int userId)
        {
            if (ActiveLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
            {
                if (LobbyLeaders.TryGetValue(lobbyId, out var leaderId) && leaderId == userId)
                {
                    List<int> remainingUsers = lobby.Keys.Where(k => k != userId).ToList();
                    if (remainingUsers.Any())
                    {
                        var newLeaderId = remainingUsers[0];
                        LobbyLeaders.TryUpdate(lobbyId, newLeaderId, userId);

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
                        ActiveLobbiesDict.TryRemove(lobbyId, out _);
                        LobbyLeaders.TryRemove(lobbyId, out _);
                    }
                }

                RemoveClientFromLobby(lobbyId, userId);
            }

            LobbyUsersDetails.TryRemove(userId, out _);

        }
        public async void StartGame(int lobbyId)
        {
            if (ActiveLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
            {
                var gameId = lobbyId;
                var gameInstance = new Game
                {
                    GameId = gameId,
                    Players = new Dictionary<int, PlayerState>(),
                    Deck = new ConcurrentStack<Card>(),
                    BabyPiles = new Dictionary<int, Stack<Card>>(),
                    ActionsPerTurn = 2 // This can be changed for each player if we have time
                };

                // Initialize the game


                ActiveGames.TryAdd(gameId, gameInstance);

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
                ActiveLobbiesDict.TryRemove(lobbyId, out _);
            }
        }
        public void ChangeReadyStatus(int lobbyId, int userId)
        {
            if (LobbyUsersDetails.TryGetValue(userId, out UserDTO userDto))
            {
                userDto.IsReady = !userDto.IsReady;

                if (ActiveLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
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
            return ActiveLobbiesDict.ContainsKey(lobbyId);
        }

        public bool IsLobbyFull(int lobbyId)
        {
            if (ActiveLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
            {
                return lobby.Count >= 4;
            }
            return false;
        }

    }
}
