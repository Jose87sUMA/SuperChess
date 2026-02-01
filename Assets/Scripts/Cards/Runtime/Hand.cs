using System.Collections.Generic;

namespace Cards.Runtime
{
    public sealed class Hand
    {
        public readonly List<CardInstance> Cards = new();
        public void Add(CardInstance c) => Cards.Add(c);
        public void Remove(CardInstance c) => Cards.Remove(c);
    }
}