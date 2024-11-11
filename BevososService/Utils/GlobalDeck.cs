using System;
using System.Collections.Generic;
using BevososService.GameModels;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BevososService
{
    public class GlobalDeck
    {

        protected GlobalDeck() { }

        public static Dictionary<int, Card> Deck { get; private set; }

        public static void InitializeDeck()
        {
            Deck = new Dictionary<int, Card>();

            int cardId = 1; 

            // 1. BODY PARTS

            // a. Any Head: 3 cartas
            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = 0, // Head
                    Damage = 4 // Asumiendo 4 para BodyParts
                });
                cardId++;
            }

            // b. Water Head: 5 cartas
            for (int i = 0; i < 5; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Water,
                    BodyPartIndex = 0, // Head
                    Damage = 3
                });
                cardId++;
            }

            // c. Land Head: 5 cartas
            for (int i = 0; i < 5; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Land,
                    BodyPartIndex = 0, // Head
                    Damage = 3
                });
                cardId++;
            }

            // d. Air Head: 5 cartas
            for (int i = 0; i < 5; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Air,
                    BodyPartIndex = 0, // Head
                    Damage = 3
                });
                cardId++;
            }

            // e. Torso: 18 cartas
            for (int i = 0; i < 18; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any, // Any para Torso
                    BodyPartIndex = 1, // Torso
                    Damage = 3
                });
                cardId++;
            }

            // f. Arms: 12 cartas (6 LeftArm y 6 RightArm)
            for (int i = 0; i < 6; i++)
            {
                // LeftArm
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = 2, // LeftArm
                    Damage = 2
                });
                cardId++;
            }
            for (int i = 0; i < 6; i++)
            {
                // RightArm
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = 4, // RightArm
                    Damage = 2
                });
                cardId++;
            }

            // g. Legs: 10 cartas
            for (int i = 0; i < 10; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.BodyPart,
                    Element = Card.CardElement.Any,
                    BodyPartIndex = 6, // Legs
                    Damage = 2
                });
                cardId++;
            }

            // 2. TOOLS

            // a. Hat: 2 cartas
            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Hat,
                    Element = Card.CardElement.Any, // Asumiendo Any para Hat
                    BodyPartIndex = 7, // Hat
                    Damage = 0
                });
                cardId++;
            }

            // b. Tools: 2 cartas (LeftTool y RightTool)
            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Tool,
                    Element = Card.CardElement.Any, // Asumiendo Any para Tools
                    BodyPartIndex = i % 2 == 0 ? 3 : 5, // LeftTool o RightTool
                    Damage = 0
                });
                cardId++;
            }

            // 3. BABYS

            // a. 1 Point Babys: 3 Land, 3 Air, 3 Water
            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Land,
                    BodyPartIndex = -1, // No aplica
                    Damage = 1
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
                    BodyPartIndex = -1, // No aplica
                    Damage = 1
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
                    BodyPartIndex = -1, // No aplica
                    Damage = 1
                });
                cardId++;
            }

            // b. 2 Point Babys: 3 Land, 3 Air, 3 Water
            for (int i = 0; i < 3; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Land,
                    BodyPartIndex = -1, // No aplica
                    Damage = 2
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
                    BodyPartIndex = -1, // No aplica
                    Damage = 2
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
                    BodyPartIndex = -1, // No aplica
                    Damage = 2
                });
                cardId++;
            }

            // c. 3 Point Babys: 2 Land, 2 Air, 2 Water
            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.Baby,
                    Element = Card.CardElement.Land,
                    BodyPartIndex = -1, 
                    Damage = 3
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
                    BodyPartIndex = -1, 
                    Damage = 3
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
                    BodyPartIndex = -1, 
                    Damage = 3
                });
                cardId++;
            }

            // 4. 2 WILD PROVOKE
            for (int i = 0; i < 2; i++)
            {
                Deck.Add(cardId, new Card
                {
                    CardId = cardId,
                    Type = Card.CardType.WildProvoke,
                    Element = Card.CardElement.Any, 
                    BodyPartIndex = -1, 
                    Damage = 0
                });
                cardId++;
            }

            // Verificación final del número de cartas
            if (Deck.Count != 88)
            {
                Console.WriteLine($"Inicialización del mazo falló. Se esperaban 88 cartas, pero se encontraron {Deck.Count}.");
            }
        }
    }

}
