using System;
using System.Collections.Generic;
using BevososService.GameModels;

namespace BevososService.Implementations
{

    public partial class ServiceImplementation : ICardManager
    {
        private static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
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
