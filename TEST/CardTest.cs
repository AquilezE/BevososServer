using BevososService;
using BevososService.GameModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TEST
{
    public class CardTest
    {
        [Fact]
        public void InitializeDeckMakes86CardsWhenUsed()
        {
            GlobalDeck.InitializeDeck();

            Assert.Equal(86, GlobalDeck.Deck.Count);
        }

        [Fact]
        public void InitializeDeck_ReturnsTrueWhenExpectedCardIsAExistingCard()
        {
            GlobalDeck.InitializeDeck();

            GlobalDeck.Deck.TryGetValue(18, out var card);

            var expectedCard = new Card
            {
                CardId = 18,
                Type = Card.CardType.Head,
                Element = Card.CardElement.Air,
                BodyPartIndex = Card.HeadIndex,
                Damage = Card.DamageTotal3
            };

            Assert.Equal(expectedCard, card);
        }
    }
}
