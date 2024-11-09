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
        //[DataMember]
        //public List<CardDTO> Deck { get; set; }

        //0 is the top babie of Land
        //1 is the top babie of Water
        //2 is the top babie of Air
        [DataMember]
        public List<CardDTO> BabyDeck { get; set; }

        [DataMember]
        public List<KeyValuePair<int, PlayerStateDTO>> playerState { get; set;}

        [DataMember]
        public int CurrentPlayerId { get; set; }

        [DataMember]
        public int ActionsRemaining { get; set; }

        public GameStateDTO() { }


        public static explicit operator GameStateDTO(GameModels.Game game)
        {

            GameStateDTO gameStateDto = new GameStateDTO();

            gameStateDto.GameStateId = game.GameId;
            //gameStateDto.Deck = game.Deck.Select(card => (CardDTO)card).ToList();
            for (int i = 0; i < 3; i++)
            {
                game.BabyPiles.TryGetValue(i, out Stack<GameModels.Card> babyPile);
                if (babyPile.Count > 0)
                {
                    gameStateDto.BabyDeck.Add((CardDTO)babyPile.Peek());
                }
                gameStateDto.BabyDeck.Add(new CardDTO { CardId = 0 });
            }

            gameStateDto.playerState = game.Players.Select(player => new KeyValuePair<int, PlayerStateDTO>(player.Key, (PlayerStateDTO)player.Value)).ToList();
            gameStateDto.CurrentPlayerId = game.CurrentPlayerId;
            gameStateDto.ActionsRemaining = game.ActionsRemaining;


            return gameStateDto;
            
        }

    }
}
