using BevososService.DTOs;
using BevososService.GameModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Utils;

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
            try
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
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
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
                    catch (CommunicationException ex)
                    {
                        ExceptionManager.LogErrorException(ex);
                        RemoveLobbyClient(user.Value);
                    }
                    catch (TimeoutException ex)
                    {
                        ExceptionManager.LogErrorException(ex);
                        RemoveLobbyClient(user.Value);
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.LogFatalException(ex);
                        RemoveLobbyClient(user.Value);
                    }
                }
            }
        }

        public void LeaveLobby(int lobbyId, int userId)
        {
            try
            {
                HandleUserLeavingLobby(lobbyId, userId);
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
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
            try
            {
                if (!LobbyLeaders.TryGetValue(lobbyId, out var leaderId) || leaderId != kickerId) return;
                if (!ActiveLobbiesDict.TryGetValue(lobbyId,
                        out ConcurrentDictionary<int, ILobbyManagerCallback> lobby)) return;
                if (!lobby.TryGetValue(targetUserId, out ILobbyManagerCallback targetCallback)) return;
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
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
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

        private void LobbyChannel_Closed(object sender, EventArgs e)
        {
            try
            {
                var callback = (ILobbyManagerCallback)sender;
                RemoveLobbyClient(callback);
                Console.WriteLine("Client Closed");
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
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

        private void LobbyChannel_Faulted(object sender, EventArgs e)
        {
            try
            {
                var callback = (ILobbyManagerCallback)sender;
                RemoveLobbyClient(callback);
                Console.WriteLine("Client Faulted");
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
                var callback = (ILobbyManagerCallback)sender;
                RemoveLobbyClient(callback);
            }
            catch (TimeoutException ex)
            {
                ExceptionManager.LogErrorException(ex);
                var callback = (ILobbyManagerCallback)sender;
                RemoveLobbyClient(callback);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                var callback = (ILobbyManagerCallback)sender;
                RemoveLobbyClient(callback);
            }
        }

        private void RemoveLobbyClient(ILobbyManagerCallback callback)
        {
            try
            {
                if (ClientCallbackMapping.TryRemove(callback, out (int LobbyId, int UserId) clientInfo))
                {
                    var lobbyId = clientInfo.LobbyId;
                    var userId = clientInfo.UserId;

                    HandleUserLeavingLobby(lobbyId, userId);
                }
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
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
                        catch (CommunicationException ex)
                        {
                            ExceptionManager.LogErrorException(ex);
                            RemoveLobbyClient(user);
                        }
                        catch (TimeoutException ex)
                        {
                            ExceptionManager.LogErrorException(ex);
                            RemoveLobbyClient(user);
                        }
                        catch (Exception ex)
                        {
                            ExceptionManager.LogFatalException(ex);
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
                            catch (CommunicationException ex)
                            {
                                ExceptionManager.LogErrorException(ex);
                                RemoveLobbyClient(user);
                            }
                            catch (TimeoutException ex)
                            {
                                ExceptionManager.LogErrorException(ex);
                                RemoveLobbyClient(user);
                            }
                            catch (Exception ex)
                            {
                                ExceptionManager.LogFatalException(ex);
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
                    ActionsPerTurn = 2
                };

                ActiveGames.TryAdd(gameId, gameInstance);

                InitializeGame(gameInstance, lobby);

                foreach (ILobbyManagerCallback kvp in lobby.Select(x => x.Value))
                {
                    ILobbyManagerCallback lobbyCallback = kvp;
                    try
                    {
                        lobbyCallback.GameStarted(gameId);
                    }
                    catch (CommunicationException ex)
                    {
                        ExceptionManager.LogErrorException(ex);
                        RemoveLobbyClient(lobbyCallback);
                    }
                    catch (TimeoutException ex)
                    {
                        ExceptionManager.LogErrorException(ex);
                        RemoveLobbyClient(lobbyCallback);
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.LogFatalException(ex);
                        RemoveLobbyClient(lobbyCallback);
                    }
                }
                await Task.Delay(5000);
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
                        catch (CommunicationException ex)
                        {
                            ExceptionManager.LogErrorException(ex);
                            RemoveLobbyClient(user);
                        }
                        catch (TimeoutException ex)
                        {
                            ExceptionManager.LogErrorException(ex);
                            RemoveLobbyClient(user);
                        }
                        catch (Exception ex)
                        {
                            ExceptionManager.LogFatalException(ex);
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
            try
            {
                return ActiveLobbiesDict.ContainsKey(lobbyId);
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                ExceptionManager.LogErrorException(ex);
                return false;
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                return false;
            }
        }

        public bool IsLobbyFull(int lobbyId)
        {
            try
            {
                if (ActiveLobbiesDict.TryGetValue(lobbyId, out ConcurrentDictionary<int, ILobbyManagerCallback> lobby))
                {
                    return lobby.Count >= 4;
                }

                return false;
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                ExceptionManager.LogErrorException(ex);
                return false;
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                return false;
            }
        }

    }
}
