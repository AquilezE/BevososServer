using BevososService.DTOs;
using BevososService.GameModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.DAO;

namespace BevososService.Implementations
{

    //NEEDS STEROID CALLBACK EXCEPTION HANDLING
    public partial class ServiceImplementation : IGameManager
    {
        private static int GetTurnTimeRemainingInSeconds(Game gameInstance)
        {
            if (gameInstance.TurnTimer == null)
            {
                return 0;
            }

            TimeSpan elapsedTime = DateTime.UtcNow - gameInstance.TurnStartTime;
            var turnDurationInSeconds = 60;
            int timeRemaining = turnDurationInSeconds - (int)elapsedTime.TotalSeconds;
            return timeRemaining > 0 ? timeRemaining : 0;

        }
        private static void BroadcastGameState(int matchCode)
        {
            if (ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                var gameStateDto = (GameStateDTO)gameInstance;

                gameStateDto.TurnTimeRemainingInSeconds = GetTurnTimeRemainingInSeconds(gameInstance);
                gameStateDto.PlayerStatistics = PlayerStatistics[matchCode].ToDictionary(kvp => kvp.Key, kvp => (GameStatsDTO)kvp.Value);

                foreach (IGameManagerCallback playerCallback in GamePlayerCallBack[matchCode].Values)
                {
                    try
                    {
                        playerCallback.ReceiveGameState(gameStateDto);
                    }
                    catch (Exception ex)
                    {
                        RemoveGameClient(playerCallback);
                    }
                }
            }
        }
        private static void GameChannel_Closed(object sender, EventArgs e)
        {
            var callback = (IGameManagerCallback)sender;
            RemoveGameClient(callback);
        }

        private static void RemoveGameClient(IGameManagerCallback callback)
        {
            foreach (KeyValuePair<int, ConcurrentDictionary<int, IGameManagerCallback>> gameEntry in GamePlayerCallBack)
            {
                int gameId = gameEntry.Key;
                ConcurrentDictionary<int, IGameManagerCallback> playerCallbacks = gameEntry.Value;

                KeyValuePair<int, IGameManagerCallback> playerToRemove = playerCallbacks.FirstOrDefault(kvp => kvp.Value == callback);

                if (!playerToRemove.Equals(default(KeyValuePair<int, IGameManagerCallback>)))
                {
                    int userId = playerToRemove.Key;
                    playerCallbacks.TryRemove(userId, out _);

                    Console.WriteLine($"Player {userId} marked as disconnected in game {gameId}");

                    if (ActiveGames.TryGetValue(gameId, out Game gameInstance))
                    {
                        if (gameInstance.Players.TryGetValue(userId, out PlayerState playerState))
                        {
                            playerState.Disconnected = true;
                        }

                        int connectedPlayers = gameInstance.Players.Values.Count(player => !player.Disconnected);

                        if (connectedPlayers < 2)
                        {
                            Console.WriteLine($"All players disconnected from game {gameId}");
                            EndGameWithNoUsers(gameId);
                        }

                    }

                    return;
                }
            }
            Console.WriteLine("Callback not found in any active game.");
        }

 

        //GameId-> GameInstance
        private static readonly ConcurrentDictionary<int, Game> ActiveGames = new ConcurrentDictionary<int, Game>();
        //GameId -> (UserId -> Callback)
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, IGameManagerCallback>> GamePlayerCallBack = new ConcurrentDictionary<int, ConcurrentDictionary<int, IGameManagerCallback>>();
        //GameId ->(UserId -> Stats)
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Stats>> PlayerStatistics = new ConcurrentDictionary<int, ConcurrentDictionary<int, Stats>>();

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
            foreach (int userId in lobby.Select(x => x.Key))
            {
                UserDTO userDto = LobbyUsersDetails[userId];

                var playerState = new PlayerState
                {
                    User = userDto,
                    Hand = new List<Card>(),
                    Monsters = new List<Monster>()
                };


                // Draw initial hand (e.g., 5 cards per player)
                for (var i = 0; i < 35; i++)
                {
                    if (gameInstance.Deck.TryPop(out Card card))
                    {
                        playerState.Hand.Add(card);
                    }
                }


                gameInstance.Players.Add(userId, playerState);
                gameInstance.PlayerActionsRemaining[userId] = gameInstance.ActionsPerTurn;

                PlayerStatistics.TryAdd(gameInstance.GameId, new ConcurrentDictionary<int, Stats>());
                PlayerStatistics[gameInstance.GameId].TryAdd(userId, new Stats());

            }

            // 4. Set the Current Player
            gameInstance.CurrentPlayerId = gameInstance.Players.Keys.First();
            gameInstance.TurnStartTime = DateTime.UtcNow;
            StartTurnTimer(gameInstance.GameId, gameInstance.CurrentPlayerId);

        }

        public void JoinGame(int gameId, UserDTO userDto)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();
            var clientChannel = (ICommunicationObject)callback;

            clientChannel.Closed += GameChannel_Closed;
            clientChannel.Faulted += GameChannel_Closed;

            if (ActiveGames.TryGetValue(gameId, out Game gameInstance))
            {
                if (!GamePlayerCallBack.ContainsKey(gameId))
                {
                    GamePlayerCallBack.TryAdd(gameId, new ConcurrentDictionary<int, IGameManagerCallback>());
                }

                GamePlayerCallBack[gameId].TryAdd(userDto.UserId, callback);

                BroadcastGameState(gameId);
            }
            else
            {
                throw new FaultException("Game does not exist.");
            }
        }
        private static void EndGame(int gameId)
        {
            SaveStatsForAllPLayers(gameId);

            var gameStats = new List<StatsDTO>();

            foreach (PlayerState playerStats in ActiveGames[gameId].Players.Values)
            {
                int points = PlayerStatistics[gameId][playerStats.User.UserId].PointsThisGame;
                string username = playerStats.User.Username;

                var statsDTO = new StatsDTO
                {
                    PointsThisGame = points,
                    Username = username
                };

                gameStats.Add(statsDTO);
            }

            if (ActiveGames.TryRemove(gameId, out Game gameInstance))
            {
                gameInstance.TurnTimer?.Dispose();

                foreach (IGameManagerCallback playerCallback in GamePlayerCallBack[gameId].Values)
                {
                    try
                    {
                        Console.WriteLine($"Game {gameId} ended");
                        playerCallback.OnNotifyGameEnded(gameId, gameStats);
                    }
                    catch (Exception ex)
                    {
                        RemoveGameClient(playerCallback);
                    }
                }


                GamePlayerCallBack.TryRemove(gameId, out _);
                PlayerStatistics.TryRemove(gameId, out _);

                Console.WriteLine($"Game {gameId} has been ended and removed from active games");
            }
        }

        private static void EndGameWithNoUsers(int gameId)
        {
            if (ActiveGames.TryRemove(gameId, out Game gameInstance))
            {
                gameInstance.TurnTimer?.Dispose();

                foreach (IGameManagerCallback playerCallback in GamePlayerCallBack[gameId].Values)
                {
                    try
                    {
                        Console.WriteLine($"Game {gameId} ended");
                        playerCallback.OnNotifyGameEndedWithoutUsers(gameId);
                    }
                    catch (Exception ex)
                    {
                        RemoveGameClient(playerCallback);
                    }
                }

                GamePlayerCallBack.TryRemove(gameId, out _);
                PlayerStatistics.TryRemove(gameId, out _);

                Console.WriteLine($"Game {gameId} has been ended and removed from active games");
            }
        }

        public void DrawCard(int matchCode, int userId)
        {
            if (!ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                return;
            }

            if (gameInstance.CurrentPlayerId != userId)
            {
                NotifyPlayer(matchCode, userId, "NotYourTurn");
                return;
            }

            if (gameInstance.PlayerActionsRemaining[userId] <= 0)
            {
                NotifyPlayer(matchCode, userId, "NoActionsRemaining");
                return;
            }

            if (gameInstance.Deck.TryPop(out Card card))
            {

                gameInstance.Players[userId].Hand.Add(card);
                gameInstance.PlayerActionsRemaining[userId]--;

                BroadcastGameState(matchCode);

                if (gameInstance.PlayerActionsRemaining[userId] == 0)
                {
                    AdvanceTurn(matchCode);
                }

                if (gameInstance.Deck.Count == 0 && !gameInstance.IsEndGamePhase)
                {
                    InitiateEndGamePhase(matchCode);
                }
            }
            else
            {
                NotifyPlayer(matchCode, userId, "DeckEmpty");
            }

        }
        public async void PlayCard(int userId, int matchCode, int cardId)
        {
            if (!ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                return;
            }

            if (!GlobalDeck.Deck.TryGetValue(cardId, out Card card))
            {
                NotifyPlayer(matchCode, userId, "InvalidCard");
                return;
            }

            if (gameInstance.CurrentPlayerId != userId)
            {
                NotifyPlayer(matchCode, userId, "NotYourTurn");
                return;
            }

            if (gameInstance.PlayerActionsRemaining[userId] <= 0)
            {
                NotifyPlayer(matchCode, userId, "NoActionsRemaining");
                return;
            }

            switch (card.Type)
            {
                case Card.CardType.Baby:
                    PlayBaby(userId, matchCode, card);

                    break;
                case Card.CardType.Head:
                    if (gameInstance.Players[userId].Monsters.Count < 3)
                    {
                        PlayHead(userId, matchCode, card);

                    }
                    else
                    {
                        NotifyPlayer(matchCode, userId, "TooManyMonsters");
                    }
                    break;
                case Card.CardType.WildProvoke:

                    //PlayProvoke(userId, matchCode, card);
                    break;

                case Card.CardType.BodyPart:
                    if (gameInstance.Players[userId].Monsters.Count == 0)
                    {
                        NotifyPlayer(matchCode, userId, "NoMonsters");
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            GamePlayerCallBack[matchCode].TryGetValue(userId, out IGameManagerCallback callback);
                            callback?.RequestBodyPartSelection(userId, matchCode, (CardDTO)card);
                        });
                    }
                    break;
                case Card.CardType.Tool:
                    if (gameInstance.Players[userId].Monsters.Count == 0)
                    {
                        NotifyPlayer(matchCode, userId, "NoMonsters");
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            GamePlayerCallBack[matchCode].TryGetValue(userId, out IGameManagerCallback callback);
                            callback?.RequestToolSelection(userId, matchCode, (CardDTO)card);
                        });

                    }
                    break;
                case Card.CardType.Hat:
                    if (gameInstance.Players[userId].Monsters.Count == 0)
                    {
                        NotifyPlayer(matchCode, userId, "NoMonsters");
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            GamePlayerCallBack[matchCode].TryGetValue(userId, out IGameManagerCallback callback);
                            callback?.RequestHatSelection(userId, matchCode, (CardDTO)card);
                        });
                    }
                    break;
                default:
                    NotifyPlayer(matchCode, userId, "UnknownCardType");
                    break;
            }
        }
        private static void PlayBaby(int userId, int matchCode, Card card)
        {
            switch (card.Element)
            {
                case Card.CardElement.Land:
                    ActiveGames[matchCode].BabyPiles[0].Push(card);
                    ActiveGames[matchCode].Players[userId].Hand.Remove(card);
                    break;
                case Card.CardElement.Water:
                    ActiveGames[matchCode].BabyPiles[1].Push(card);
                    ActiveGames[matchCode].Players[userId].Hand.Remove(card);
                    break;
                case Card.CardElement.Air:
                    ActiveGames[matchCode].BabyPiles[2].Push(card);
                    ActiveGames[matchCode].Players[userId].Hand.Remove(card);
                    break;
            }

            ActiveGames.TryGetValue(matchCode, out Game gameInstance);
            gameInstance.PlayerActionsRemaining[userId]--;




            if (gameInstance.PlayerActionsRemaining[userId] == 0)
            {
                if (gameInstance.IsEndGamePhase)
                {
                    gameInstance.PlayersWhoFinishedFinalTurn.Add(userId);
                }

                AdvanceTurn(matchCode);
                return;
            }
            BroadcastGameState(matchCode);

        }
        private static void PlayHead(int userId, int matchCode, Card card)
        {
            if (ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {

                var monster = new Monster
                {
                    Head = card,
                    Torso = null,
                    LeftArm = null,
                    LeftHandTool = null,
                    RightArm = null,
                    RightHandTool = null,
                    Legs = null,
                    Hat = null
                };
                gameInstance.Players[userId].Monsters.Add(monster);
                gameInstance.Players[userId].Hand.Remove(card);

                PlayerStatistics[gameInstance.GameId][userId].MonstersCreated++;
                
                gameInstance.PlayerActionsRemaining[userId]--;


                if (gameInstance.PlayerActionsRemaining[userId] == 0)
                {
                    if (gameInstance.IsEndGamePhase)
                    {
                        gameInstance.PlayersWhoFinishedFinalTurn.Add(userId);
                    }

                    AdvanceTurn(matchCode);
                    return;
                }
                BroadcastGameState(matchCode);

            }

        }
        public void PlayProvoke(int userId, int matchCode, int babyPileIndex)
        {
            Console.WriteLine($"PlayProvoke called with userId: {userId}, matchCode: {matchCode}, babyPileIndex: {babyPileIndex}");

            if (!ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                Console.WriteLine("Game not found");
                return;
            }

            if (gameInstance.CurrentPlayerId != userId)
            {
                NotifyPlayer(matchCode, userId, "NotYourTurn");
                return;
            }

            if (gameInstance.Players[userId].ActionsPerTurn > gameInstance.PlayerActionsRemaining[userId])
            {
                NotifyPlayer(matchCode, userId, "NoActionsRemaining");
                return;
            }

            ExecuteProvoke(userId, matchCode, babyPileIndex);

        }
        public void ExecuteBodyPartPlacement(int userId, int matchCode, int cardId, int monsterSelectedIndex)
        {
            if (!ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                return;
            }

            if (!GlobalDeck.Deck.TryGetValue(cardId, out Card card))
            {
                NotifyPlayer(matchCode, userId, "InvalidCard");
                return;
            }

            if (monsterSelectedIndex >= 0 && monsterSelectedIndex < gameInstance.Players[userId].Monsters.Count)
            {


                if (gameInstance.Players[userId].Monsters[monsterSelectedIndex].AddPart(card))
                {
                    gameInstance.Players[userId].Hand.Remove(card);

                    gameInstance.PlayerActionsRemaining[userId]--;

                    if (gameInstance.PlayerActionsRemaining[userId] == 0)
                    {
                        if (gameInstance.IsEndGamePhase)
                        {
                            gameInstance.PlayersWhoFinishedFinalTurn.Add(userId);
                        }

                        AdvanceTurn(matchCode);
                        return;
                    }
                    BroadcastGameState(matchCode);

                }
                else
                {
                    NotifyPlayer(matchCode, userId, "PartAlreadyExists");
                }
            }
            else
            {
                NotifyPlayer(matchCode, userId, "InvalidMonsterSelection");
            }


        }
        public void ExecuteToolPlacement(int userId, int matchCode, int cardId, int monsterSelectedIndex)
        {
            if (!ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                return;
            }

            if (!GlobalDeck.Deck.TryGetValue(cardId, out Card card))
            {
                NotifyPlayer(matchCode, userId, "InvalidCard");

                return;
            }

            if (monsterSelectedIndex >= 0 && monsterSelectedIndex < gameInstance.Players[userId].Monsters.Count)
            {

                if (gameInstance.Players[userId].Monsters[monsterSelectedIndex].AddPart(card))
                {

                    gameInstance.Players[gameInstance.CurrentPlayerId].ActionsPerTurn++;
                    gameInstance.PlayerActionsRemaining[userId]--;
                    gameInstance.Players[userId].Hand.Remove(card);

                    BroadcastGameState(matchCode);

                    if (gameInstance.PlayerActionsRemaining[userId] == 0)
                    {
                        if (gameInstance.IsEndGamePhase)
                        {
                            gameInstance.PlayersWhoFinishedFinalTurn.Add(userId);
                        }
                        AdvanceTurn(matchCode);
                    }
                }
                else
                {
                    NotifyPlayer(matchCode, userId, "PartAlreadyExists");
                }

            }
            else
            {
                NotifyPlayer(matchCode, userId, "InvalidMonsterSelection");
            }

        }
        public void ExecuteHatPlacement(int userId, int matchCode, int cardId, int monsterSelectedIndex)
        {
            if (!ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                return;
            }

            if (!GlobalDeck.Deck.TryGetValue(cardId, out Card card))
            {
                NotifyPlayer(matchCode, userId, "InvalidCard");
                return;
            }

            if (monsterSelectedIndex >= 0 && monsterSelectedIndex < gameInstance.Players[userId].Monsters.Count)
            {

                if (gameInstance.Players[userId].Monsters[monsterSelectedIndex].AddPart(card))
                {
                    gameInstance.Players[userId].Hand.Remove(card);

                    gameInstance.PlayerActionsRemaining[userId]--;


                    BroadcastGameState(matchCode);

                    if (gameInstance.PlayerActionsRemaining[userId] == 0)
                    {
                        if (gameInstance.IsEndGamePhase)
                        {
                            gameInstance.PlayersWhoFinishedFinalTurn.Add(userId);
                        }

                        AdvanceTurn(matchCode);
                    }
                }
                else
                {
                    NotifyPlayer(matchCode, userId, "You have no head");
                }

            }
            else
            {
                NotifyPlayer(matchCode, userId, "InvalidMonsterSelection");
            }
        }
        public void ExecuteProvoke(int userId, int matchCode, int babyPileIndex)
        {

            Console.WriteLine($"PlayProvoke called with userId: {userId}, matchCode: {matchCode}, babyPileIndex: {babyPileIndex}");


            if (!ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                return;
            }

            if (gameInstance.BabyPiles[babyPileIndex].Count == 0)
            {
                NotifyPlayer(matchCode, userId, "EmptyBabyPile");
                return;
            }

            if(gameInstance.CurrentPlayerId != userId)
            {
                NotifyPlayer(matchCode, userId, "NotYourTurn");
                return;
            }

            if (gameInstance.PlayerActionsRemaining[userId] < gameInstance.Players[userId].ActionsPerTurn)
            {
                NotifyPlayer(matchCode, userId, "NoActionsRemaining");
                return;
            }


            CallOnProvokeForAllPlayers(matchCode, babyPileIndex);

            gameInstance.PlayerActionsRemaining[userId] = 0;


            Card.CardElement element = gameInstance.BabyPiles[babyPileIndex].Peek().Element;

            var monsterArmyMaxStrenght = 0;
            var playerWithMaxStrenght = 0;

            var pileStrenght = 0;
            int numberBabies = gameInstance.BabyPiles[babyPileIndex].Count;

            foreach (Card baby in gameInstance.BabyPiles[babyPileIndex])
            {
                pileStrenght += baby.Damage;
            }

            foreach (PlayerState player in gameInstance.Players.Values)
            {
                var monsterArmyStrenght = 0;
                var monstersToRemove = new List<Monster>();

                foreach (Monster monster in player.Monsters)
                {
                    if (monster.Head.Element == element || monster.Head.Element == Card.CardElement.Any)
                    {
                        monsterArmyStrenght += monster.GetDamage();
                        monstersToRemove.Add(monster);
                    }
                }

                foreach (Monster monster in monstersToRemove)
                {
                    if (monster.Hat != null)
                    {
                        gameInstance.Players[player.User.UserId].ActionsPerTurn--;

                        if (gameInstance.PlayerActionsRemaining[player.User.UserId] > gameInstance.Players[player.User.UserId].ActionsPerTurn)
                        {
                            gameInstance.PlayerActionsRemaining[player.User.UserId] = gameInstance.Players[player.User.UserId].ActionsPerTurn;
                        }
                    }
                    player.Monsters.Remove(monster);
                }

                if (monsterArmyStrenght > monsterArmyMaxStrenght)
                {
                    monsterArmyMaxStrenght = monsterArmyStrenght;
                    playerWithMaxStrenght = player.User.UserId;
                }
            }

            if (pileStrenght > monsterArmyMaxStrenght)
            {
                gameInstance.BabyPiles[babyPileIndex].Clear();
            }
            else if (pileStrenght <= monsterArmyMaxStrenght)
            {
                PlayerStatistics[gameInstance.GameId][playerWithMaxStrenght].AnihilatedBabies += numberBabies;
                PlayerStatistics[gameInstance.GameId][playerWithMaxStrenght].PointsThisGame += pileStrenght;
                gameInstance.BabyPiles[babyPileIndex].Clear();
            }


            if (gameInstance.PlayerActionsRemaining[userId] == 0)
            {
                if (gameInstance.IsEndGamePhase)
                {
                    gameInstance.PlayersWhoFinishedFinalTurn.Add(userId);
                }

                AdvanceTurn(matchCode);
            }

        }
        private static void AdvanceTurn(int matchCode)
        {
            if (ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                List<int> playerIds = gameInstance.Players.Keys.ToList();
                int currentIndex = playerIds.IndexOf(gameInstance.CurrentPlayerId);
                int nextIndex = (currentIndex + 1) % playerIds.Count;
                gameInstance.CurrentPlayerId = playerIds[nextIndex];

                int actionsPerTurn = gameInstance.Players[gameInstance.CurrentPlayerId].ActionsPerTurn;
                gameInstance.PlayerActionsRemaining[gameInstance.CurrentPlayerId] = actionsPerTurn;

                gameInstance.TurnStartTime = DateTime.UtcNow;


                StartTurnTimer(matchCode, gameInstance.CurrentPlayerId);

                BroadcastGameState(matchCode);

                if (gameInstance.IsEndGamePhase)
                {
                    CheckEndGame(matchCode);
                }
            }
        }
        private static void StartTurnTimer(int matchCode, int userId)
        {
            var turnDurationInSeconds = 60;

            if (!ActiveGames.TryGetValue(matchCode, out Game gameInstance)) return;

            gameInstance.TurnTimer?.Dispose();

            gameInstance.Players.TryGetValue(userId, out PlayerState playerState);

            if (playerState.Disconnected) turnDurationInSeconds = 5;


            gameInstance.TurnTimer = new Timer(
                callback: state =>
                {
                    //advance turn
                    Console.WriteLine($"Player {userId}'s turn has timed out.");
                    AdvanceTurn(matchCode);
                },
                state: null,
                dueTime: turnDurationInSeconds * 1000,
                period: Timeout.Infinite
            );
        }

        public static void InitiateEndGamePhase(int matchCode)
        {
            if (ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                gameInstance.IsEndGamePhase = true;
                Console.WriteLine($"Game {matchCode} ");
            }

            foreach (IGameManagerCallback gameManagerCallback in GamePlayerCallBack[matchCode].Values)
            {
                try
                {
                    gameManagerCallback.OnNotifyEndGamePhase();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error notifying player: {e.Message}");
                    
                    RemoveGameClient(gameManagerCallback);
                }
            }
        }

        public static void CheckEndGame(int matchCode)
        {
            if(ActiveGames.TryGetValue(matchCode, out Game gameInstance))
            {
                bool allActivePlayersFinished = gameInstance.Players.Where(kvp => !kvp.Value.Disconnected)
                    .All(kvp => gameInstance.PlayersWhoFinishedFinalTurn.Contains(kvp.Key));
                
                if (allActivePlayersFinished)
                {
                    EndGame(matchCode);
                }
            }
        }

        private static void NotifyPlayer(int matchCode, int userId, string messageKey)
        {
            if (GamePlayerCallBack.TryGetValue(matchCode, out ConcurrentDictionary<int, IGameManagerCallback> playerCallbacks))
            {
                if (playerCallbacks.TryGetValue(userId, out IGameManagerCallback callback))
                {
                    callback.NotifyActionInvalid(messageKey);
                }
            }
        }
        private static void CallOnProvokeForAllPlayers(int matchCode, int babyPileIndex)
        {
            foreach (int player in GamePlayerCallBack[matchCode].Keys)
            {
                GamePlayerCallBack[matchCode][player].OnProvoke(matchCode, babyPileIndex);
            }
        }

        private static void SaveStatsForAllPLayers(int matchCode)
        {
            var maxPoints = 0;
            var winnerId = 0;

            foreach (PlayerState maxPointsPlayer in ActiveGames[matchCode].Players.Values)
            {
                int points = PlayerStatistics[matchCode][maxPointsPlayer.User.UserId].PointsThisGame;

                if (points > maxPoints)
                {
                    maxPoints = points;
                    winnerId = maxPointsPlayer.User.UserId;
                }
            }

            foreach (PlayerState player in ActiveGames[matchCode].Players.Values)
            {
                
                int monsters = PlayerStatistics[matchCode][player.User.UserId].MonstersCreated;
                int babies = PlayerStatistics[matchCode][player.User.UserId].AnihilatedBabies;

                if (player.User.UserId > 0)
                {
                    if (new StatsDAO().UserStatsExists(player.User.UserId))
                    {
                        DataAccess.Models.Stats userStats = new StatsDAO().GetUserStats(player.User.UserId);

                        userStats.Wins += player.User.UserId == winnerId ? 1 : 0;
                        userStats.MonstersCreated += monsters;
                        userStats.AnnihilatedBabies += babies;

                        new StatsDAO().UpdateUserStats(player.User.UserId, userStats);
                    }
                    else
                    {
                        var newUserStats = new DataAccess.Models.Stats
                        {
                            UserId = player.User.UserId,
                            Wins = player.User.UserId == winnerId ? 1 : 0,
                            MonstersCreated = monsters,
                            AnnihilatedBabies = babies
                        };

                        new StatsDAO().AddNewUserStats(player.User.UserId, newUserStats);
                    }
                }
                else
                {
                    Console.WriteLine("Guest user, cannot save Stats");
                }
                
            }


        }
    }

}
