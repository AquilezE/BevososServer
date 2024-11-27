using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BevososService.GameModels;

namespace BevososService.Implementations
{

    public partial class ServiceImplementation : ICardManager
    {
        public static void SeeGlobalDeck()
        {
            foreach (KeyValuePair<int, Card> card in GlobalDeck.Deck)
            {
                Console.WriteLine(card.Value);
            }
        }

        private static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }
}
