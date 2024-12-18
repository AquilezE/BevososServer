namespace BevososService.GameModels
{

    public class Card
    {

        public const int HeadIndex = 0;
        public const int BodyIndex = 1;
        public const int LeftArmIndex = 2;
        public const int LeftArmToolIndex = 3;
        public const int RightArmIndex = 4;
        public const int RightArmToolIndex = 5;
        public const int LegsIndex = 6;
        public const int HatIndex = 7;
        public const int BabyCardIndex = -1;

        public const int DamageTotal0 = 0;
        public const int DamageTotal1 = 1;
        public const int DamageTotal2 = 2;
        public const int DamageTotal3 = 3;
        public const int DamageTotal4 = 4;

        public int CardId { get; set; }

        public enum CardType
        {

            Baby,
            BodyPart,
            Head,
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

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var card = (Card)obj;
            return CardId == card.CardId && Damage == card.Damage && BodyPartIndex == card.BodyPartIndex &&
                   Element == card.Element;
        }

    }

}