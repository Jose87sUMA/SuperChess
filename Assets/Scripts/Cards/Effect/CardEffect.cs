using BoardLogic;
using ChessPieces;
using UnityEngine;

namespace Cards.Runtime
{
    public abstract class CardEffect
    {
        protected CardInstance Inst;          
        protected CardSystem   Sys;
        public virtual bool DeferredConfirmation => false;

        public abstract void Resolve(CardInstance inst, CardSystem cs, Chessboard b);

        public virtual void ResolveRemote(byte[] payload, CardSystem cs, Chessboard b) => Resolve(null, cs, b);

        public virtual byte[] SerializePayload() => System.Array.Empty<byte>();
        
        
        protected void CancelCard(Chessboard board, string reason)
        {
            board.ToggleHighlights(false);

            Sys.RefundCard(Inst);
            Sys.GetComponent<HandUI>()?.ShowInfo(reason);
        }
        
    }

}