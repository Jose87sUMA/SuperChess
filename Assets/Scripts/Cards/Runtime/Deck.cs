using System.Collections.Generic;
using System.Linq;
using Cards.Data;

namespace Cards.Runtime
{
    public sealed class Deck
    {
        readonly List<CardInstance> _drawPile;

        public Deck(IEnumerable<CardData> definition)
        {
            _drawPile = definition
                .Where(d => d)
                .Select(d => new CardInstance(d))
                .ToList();
            Shuffle(_drawPile);
        }

        public CardInstance Draw()
        {
            if (_drawPile.Count == 0) return null;
            var c = _drawPile[^1];
            _drawPile.RemoveAt(_drawPile.Count-1);
            return c;
        }

        public int Count => _drawPile.Count;

        static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}