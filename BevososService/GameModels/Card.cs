using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.GameModels
{
    public class Card
    {

        public int CardId { get; set; }

        public enum CardType
        {
            Baby,
            BodyPart,
            WildProvoke,
            Hat,
            Tool
        }

        public enum CardElement
        {
            Land,
            Water,
            Air,
            Any
        }
        

        public int Damage { get; set; }
        public CardType Type { get; set; }
        public CardElement Element { get; set; }

        public int BodyPartIndex { get; set; }


    }
}
