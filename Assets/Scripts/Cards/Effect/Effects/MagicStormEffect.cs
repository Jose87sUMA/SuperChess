using BoardLogic;
using Cards.Runtime;
using UnityEngine;

namespace Cards.Effect.Effects
{

    public sealed class MagicStormEffect : CardEffect
    {
        public override byte[] SerializePayload() => System.Array.Empty<byte>();

        public override void Resolve(CardInstance _, CardSystem cs, Chessboard board)
        {
            board.SetGlobalMoveCap(2, 2);
        }

        public override void ResolveRemote(byte[] _, CardSystem cs, Chessboard board){
            Resolve(null, cs, board);
        }
    }
}