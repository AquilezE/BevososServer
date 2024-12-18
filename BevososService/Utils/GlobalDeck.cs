using System.Collections.Generic;
using BevososService.GameModels;

namespace BevososService
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
        }

    }

}