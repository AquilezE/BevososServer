using System.Runtime.Serialization;

namespace BevososService.DTOs
{

    [DataContract]
    public class CardDTO
    {

        [DataMember]
        public int CardId { get; set; }

        public static explicit operator CardDTO(GameModels.Card card)
        {
            return new CardDTO
            {
                CardId = card.CardId
            };
        }

    }

}