using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.GameModels
{
    public class Game
    {
        public int GameId { get; set; }
        public Stack<Card> Deck { get; set; }

        //0 babie of Land
        //1 babie of Water
        //2 babie of Air
        public Dictionary<int, Stack<Card>> BabyPiles { get; set; } // Keyed by BabyType enum or similar identifier
        public Dictionary<int, PlayerState> Players { get; set; } = new Dictionary<int, PlayerState>();
        public int CurrentPlayerId { get; set; }
        public int ActionsRemaining { get; set; }

    }
}
