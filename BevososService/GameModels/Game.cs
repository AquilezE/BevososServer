using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace BevososService.GameModels
{

    public class Game
    {

        public int GameId { get; set; }
        public ConcurrentStack<Card> Deck { get; set; }
        public Dictionary<int, Stack<Card>> BabyPiles { get; set; }
        public Dictionary<int, PlayerState> Players { get; set; } = new Dictionary<int, PlayerState>();
        public int CurrentPlayerId { get; set; }
        public int ActionsPerTurn { get; set; } = 2;

        public ConcurrentDictionary<int, int> PlayerActionsRemaining { get; set; } =
            new ConcurrentDictionary<int, int>();

        public Timer TurnTimer { get; set; }

        public DateTime TurnStartTime { get; set; }

        public bool IsEndGamePhase { get; set; } = false;

        public HashSet<int> PlayersWhoFinishedFinalTurn { get; set; } = new HashSet<int>();

    }

}