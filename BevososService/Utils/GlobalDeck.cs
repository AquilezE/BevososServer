using System.Collections.Generic;
using BevososService.GameModels;

namespace BevososService.Utils
{

    public class GlobalDeck
    {

        protected GlobalDeck()
        {

        }

        public static Dictionary<int, Card> Deck { get; private set; }

        public static void InitializeDeck()
        {
            Deck = new Dictionary<int, Card>();

            int cardId = 1;


            cardId = InitializeBearHeads(cardId);

            cardId = InitializeWaterHeads(cardId);

            cardId = InitializeLandHeads(cardId);

            cardId = InitializeAirHeads(cardId);

            cardId = InitializeBodyParts(cardId);

            cardId = InitializeTools(cardId);

            cardId = InitializeOnePointBabies(cardId);

            cardId = InitializeTwoPointBabies(cardId);

            InitializeThreePointBabies(cardId);

        }

        private static int InitializeWaterHeads(int cardId)
        {
            for (int i = 0; i < 5; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Head,
                    Element = Card.CardElement.Water,
                    BodyPartIndex = Card.HeadIndex,
                    Damage = Card.DamageTotal3
                });
                cardId++;
            }

            return cardId;
        }

        private static int InitializeLandHeads(int cardId)
        {

            for (int i = 0; i < 5; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Head,
                    Element = Card.CardElement.Land,
                    BodyPartIndex = Card.HeadIndex,
                    Damage = Card.DamageTotal3
                });
                cardId++;
            }

            return cardId;
        }

        private static int InitializeBearHeads(int cardId)
        {
            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Head,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = Card.HeadIndex,
                    Damage = Card.DamageTotal4
                });
                cardId++;
            }

            return cardId;
        }

        private static int InitializeAirHeads(int cardId) {

            for (int i = 0; i < 5; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Head,
                    Element = Card.CardElement.Air,
                    BodyPartIndex = Card.HeadIndex,
                    Damage = Card.DamageTotal3
                });
                cardId++;
            }
            return cardId;

        }

        private static int InitializeBodyParts(int cardId)
        {

            for (int i = 0; i < 18; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = Card.BodyIndex,
                    Damage = Card.DamageTotal3
                });
                cardId++;
            }


            for (int i = 0; i < 6; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = Card.LeftArmIndex,
                    Damage = Card.DamageTotal2
                });
                cardId++;
            }

            for (int i = 0; i < 6; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = Card.RightArmIndex,
                    Damage = Card.DamageTotal2
                });
                cardId++;
            }

            for (int i = 0; i < 10; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = Card.LegsIndex,
                    Damage = Card.DamageTotal2
                });
                cardId++;
            }

            return cardId;
        }

        private static int InitializeTools(int cardId)
        {
            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Hat,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = Card.HatIndex,
                    Damage = Card.DamageTotal0
                });
                cardId++;
            }

            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Tool,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = i % 2 == 0 ? Card.LeftArmToolIndex : Card.RightArmToolIndex,
                    Damage = Card.DamageTotal0
                });
                cardId++;
            }

            return cardId;
        }

        private static int InitializeOnePointBabies(int cardId)
        {

            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Land,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal1
                });
                cardId++;
            }

            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Air,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal1
                });
                cardId++;
            }

            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Water,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal1
                });
                cardId++;
            }

            return cardId;
        }
    
        private static int InitializeTwoPointBabies(int cardId)
        {
            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Land,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal2
                });
                cardId++;
            }

            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Air,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal2
                });
                cardId++;
            }

            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Water,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal2
                });
                cardId++;
            }

            return cardId;
        }

        private static int InitializeThreePointBabies(int cardId)
        {

            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Land,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal3
                });
                cardId++;
            }

            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Air,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal3
                });
                cardId++;
            }

            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Water,
                    BodyPartIndex = Card.BabyCardIndex,
                    Damage = Card.DamageTotal3
                });
                cardId++;
            }
            return cardId;
        }  
    
    }

}