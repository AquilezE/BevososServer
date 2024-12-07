using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BevososService.GameModels;

namespace BevososService.DTOs
{
    [DataContract]
    public class GameStateDTO
    {
        [DataMember]
        public int GameStateId { get; set; }


        //0 is the top baby of Land
        //1 is the top baby of Water
        //2 is the top baby of Air
        [DataMember]
        public List<CardDTO> BabyDeck { get; set; }

        [DataMember]
        public Dictionary<int, PlayerStateDTO> playerState { get; set;} = new Dictionary<int, PlayerStateDTO>();

        [DataMember]
        public int CurrentPlayerId { get; set; }

        [DataMember]
        public Dictionary<int, int> PlayerActionsRemaining { get; set; }

        [DataMember]
        public int CardsRemainingInDeck { get; set; }

        [DataMember]
        public int TurnTimeRemainingInSeconds { get; set; }

        [DataMember]
        public Dictionary<int, GameStatsDTO> PlayerStatistics { get; set; } = new Dictionary<int, GameStatsDTO>();


        public static explicit operator GameStateDTO(Game game)
        {

            var gameStateDto = new GameStateDTO
            {
                GameStateId = game.GameId,
                BabyDeck = new List<CardDTO>()
            };

            for (var i = 0; i < 3; i++)
            {
                game.BabyPiles.TryGetValue(i, out Stack<Card> babyPile);
                if (babyPile != null && babyPile.Count > 0)
                {
                    gameStateDto.BabyDeck.Add((CardDTO)babyPile.Peek());
                }
                else
                { 
                    gameStateDto.BabyDeck.Add(new CardDTO { CardId = 0 });
                }
            }

            gameStateDto.CurrentPlayerId = game.CurrentPlayerId;
            gameStateDto.PlayerActionsRemaining = game.PlayerActionsRemaining.ToDictionary(kv => kv.Key, kv => kv.Value);

            foreach (KeyValuePair<int, PlayerState> player in game.Players)
            {
                gameStateDto.playerState.Add(player.Key, (PlayerStateDTO)player.Value);
            }


            gameStateDto.CardsRemainingInDeck = game.Deck.Count;


            return gameStateDto;
            
        }

    }
}
