using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.DTOs
{
    [DataContract]
    public class GameStateDTO
    {
        [DataMember]
        public int GameStateId { get; set; }


        //0 is the top babie of Land
        //1 is the top babie of Water
        //2 is the top babie of Air
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

        public GameStateDTO() { }


        public static explicit operator GameStateDTO(GameModels.Game game)
        {

            GameStateDTO gameStateDto = new GameStateDTO();

            gameStateDto.GameStateId = game.GameId;
            gameStateDto.BabyDeck = new List<CardDTO>();

            for (int i = 0; i < 3; i++)
            {
                game.BabyPiles.TryGetValue(i, out Stack<GameModels.Card> babyPile);
                if (babyPile.Count > 0)
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

            foreach (var player in game.Players)
            {
                gameStateDto.playerState.Add(player.Key, (PlayerStateDTO)player.Value);
            }


            gameStateDto.CardsRemainingInDeck = game.Deck.Count;


            return gameStateDto;
            
        }

    }
}
