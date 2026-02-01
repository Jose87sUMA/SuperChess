using Cards.Data;

namespace Cards.Runtime
{
    public sealed class CardInstance
    {
        public readonly CardData Data;
        public CardInstance(CardData src) => Data = src;
    }
}