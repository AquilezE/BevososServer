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
    public partial class ServiceImplementation : IGameManager
    {
        private static int GetTurnTimeRemainingInSeconds(Game gameInstance)
        {
            if (gameInstance.TurnTimer == null)
            {
                return 0;
            }

            TimeSpan elapsedTime = DateTime.UtcNow - gameInstance.TurnStartTime;
            int turnDurationInSeconds = 60;
            int timeRemaining = turnDurationInSeconds - (int)elapsedTime.TotalSeconds;
            return timeRemaining > 0 ? timeRemaining : 0;

        }
        private static void BroadcastGameState(int matchCode)
        {
            if (_activeGames.TryGetValue(matchCode, out Game gameInstance))
            {
                var gameStateDto = (GameStateDTO)gameInstance;

                gameStateDto.TurnTimeRemainingInSeconds = GetTurnTimeRemainingInSeconds(gameInstance);

                foreach (var playerCallback in _gamePlayerCallBack[matchCode].Values)
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

        //GameId-> GameInstance
        private static ConcurrentDictionary<int, Game> _activeGames = new ConcurrentDictionary<int, Game>();
        //GameId -> (UserId -> Callback)
        private static ConcurrentDictionary<int, ConcurrentDictionary<int, IGameManagerCallback>> _gamePlayerCallBack = new ConcurrentDictionary<int, ConcurrentDictionary<int, IGameManagerCallback>>();
        //GameId ->(UserId -> Stats)
        private static ConcurrentDictionary<int, ConcurrentDictionary<int, Stats>> _playerStatistics = new ConcurrentDictionary<int, ConcurrentDictionary<int, Stats>>();

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
                gameInstance.PlayerActionsRemaining[userId] = gameInstance.ActionsPerTurn;

                _playerStatistics.TryAdd(gameInstance.GameId, new ConcurrentDictionary<int, Stats>());
                _playerStatistics[gameInstance.GameId].TryAdd(userId, new Stats());

            }

            // 4. Set the Current Player
            gameInstance.CurrentPlayerId = gameInstance.Players.Keys.First();
            gameInstance.TurnStartTime = DateTime.UtcNow;
            StartTurnTimer(gameInstance.GameId, gameInstance.CurrentPlayerId);

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

                BroadcastGameState(gameId);
            }
            else
            {
                throw new FaultException("Game does not exist.");
            }
        }
        private static void EndGame(int gameId)
        {
            if (_activeGames.TryRemove(gameId, out Game gameInstance))
            {
                gameInstance.TurnTimer?.Dispose();

                _gamePlayerCallBack.TryRemove(gameId, out _);
                Console.WriteLine($"Game {gameId} has been ended and removed from active games.");
            }
        }
        public void DrawCard(int matchCode, int userId)
        {
            if (!_activeGames.TryGetValue(matchCode, out Game gameInstance))
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
            }
            else
            {
                NotifyPlayer(matchCode, userId, "DeckEmpty");
            }

        }
        public async void PlayCard(int userId, int matchCode, int cardId)
        {
            if (!_activeGames.TryGetValue(matchCode, out Game gameInstance))
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
                            _gamePlayerCallBack[matchCode].TryGetValue(userId, out IGameManagerCallback callback);
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
                            _gamePlayerCallBack[matchCode].TryGetValue(userId, out IGameManagerCallback callback);
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
                            _gamePlayerCallBack[matchCode].TryGetValue(userId, out IGameManagerCallback callback);
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
            gameInstance.PlayerActionsRemaining[userId]--;




            if (gameInstance.PlayerActionsRemaining[userId] == 0)
            {
                AdvanceTurn(matchCode);
                return;
            }
            BroadcastGameState(matchCode);

        }
        private static void PlayHead(int userId, int matchCode, Card card)
        {
            if (_activeGames.TryGetValue(matchCode, out Game gameInstance))
            {

                Monster monster = new Monster
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

                gameInstance.PlayerActionsRemaining[userId]--;


                if (gameInstance.PlayerActionsRemaining[userId] == 0)
                {
                    AdvanceTurn(matchCode);
                    return;
                }
                BroadcastGameState(matchCode);



            }

        }
        public async void PlayProvoke(int userId, int matchCode)
        {
            if (!_activeGames.TryGetValue(userId, out Game gameInstance))
            {
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

            else
            {
                await Task.Run(() =>
                {
                    _gamePlayerCallBack[matchCode].TryGetValue(userId, out IGameManagerCallback callback);
                    callback?.RequestProvokeSelection(userId, matchCode);
                });
            }
        }
        public void ExecuteBodyPartPlacement(int userId, int matchCode, int cardId, int monsterSelectedIndex)
        {
            if (!_activeGames.TryGetValue(matchCode, out Game gameInstance))
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
            if (!_activeGames.TryGetValue(matchCode, out Game gameInstance))
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
            if (!_activeGames.TryGetValue(matchCode, out Game gameInstance))
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
        public void ExecuteProvoke(int userId, int matchCode, int babyPile)
        {
            if (!_activeGames.TryGetValue(matchCode, out Game gameInstance))
            {
                return;
            }

            if (gameInstance.BabyPiles[babyPile].Count == 0)
            {
                NotifyPlayer(matchCode, userId, "EmptyBabyPile");
                return;
            }

            Card.CardElement element = gameInstance.BabyPiles[babyPile].Peek().Element;

            int monsterArmyMaxStrenght = 0;
            int playerWithMaxStrenght = 0;

            int pileStrenght = 0;
            int numberBabies = gameInstance.BabyPiles[babyPile].Count;

            foreach (Card baby in gameInstance.BabyPiles[babyPile])
            {
                pileStrenght += baby.Damage;
            }

            foreach (var player in gameInstance.Players.Values)
            {
                int monsterArmyStrenght = 0;
                List<Monster> monstersToRemove = new List<Monster>();

                foreach (var monster in player.Monsters)
                {
                    if (monster.Head.Element == element || monster.Head.Element == Card.CardElement.Any)
                    {
                        monsterArmyStrenght += monster.GetDamage();
                        monstersToRemove.Add(monster);
                    }
                }

                foreach (var monster in monstersToRemove)
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
                gameInstance.BabyPiles[babyPile].Clear();
            }
            else if (pileStrenght <= monsterArmyMaxStrenght)
            {
                _playerStatistics[gameInstance.GameId][playerWithMaxStrenght].BabiesKilledThisGame += numberBabies;
                _playerStatistics[gameInstance.GameId][playerWithMaxStrenght].PointsThisGame += pileStrenght;
                gameInstance.BabyPiles[babyPile].Clear();
            }

            BroadcastGameState(matchCode);
            AdvanceTurn(matchCode);

        }
        private static void AdvanceTurn(int matchCode)
        {
            if (_activeGames.TryGetValue(matchCode, out Game gameInstance))
            {
                var playerIds = gameInstance.Players.Keys.ToList();
                int currentIndex = playerIds.IndexOf(gameInstance.CurrentPlayerId);
                int nextIndex = (currentIndex + 1) % playerIds.Count;
                gameInstance.CurrentPlayerId = playerIds[nextIndex];

                int actionsPerTurn = gameInstance.Players[gameInstance.CurrentPlayerId].ActionsPerTurn;
                gameInstance.PlayerActionsRemaining[gameInstance.CurrentPlayerId] = actionsPerTurn;

                gameInstance.TurnStartTime = DateTime.UtcNow;


                StartTurnTimer(matchCode, gameInstance.CurrentPlayerId);

                BroadcastGameState(matchCode);
            }
        }
        private static void StartTurnTimer(int matchCode, int userId)
        {
            int turnDurationInSeconds = 60;
            var gameInstance = _activeGames[matchCode];

            if (gameInstance.TurnTimer != null)
            {
                gameInstance.TurnTimer.Dispose();
            }

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
        private static void NotifyPlayer(int matchCode, int userId, string messageKey)
        {
            if (_gamePlayerCallBack.TryGetValue(matchCode, out var playerCallbacks))
            {
                if (playerCallbacks.TryGetValue(userId, out var callback))
                {
                    callback.NotifyActionInvalid(messageKey);
                }
            }
        }
    }

}
