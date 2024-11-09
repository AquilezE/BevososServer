using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
