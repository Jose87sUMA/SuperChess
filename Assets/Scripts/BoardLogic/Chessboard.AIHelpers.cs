using ChessGame;
using ChessPieces;
using UnityEngine;

namespace BoardLogic
{
    public struct MoveContext
    {
        public Vector2Int From, To;
        public ChessPiece MovingPiece;
        public ChessPiece CapturedPiece;
        public bool EnPassant;
        public Vector2Int EnPassantSquare;
    }
    public partial class Chessboard
    { 
        
        public void MakeTemporaryMove(Vector2Int from, Vector2Int to, out MoveContext ctx)
        {
            ctx = new MoveContext
            {
                From = from,
                To = to,
                MovingPiece  = ChessPieces[from.x, from.y],
                CapturedPiece= ChessPieces[to.x, to.y]
            };            
            if (ctx.MovingPiece.type == ChessPieceType.Pawn && ctx.CapturedPiece is null && from.x != to.x && !canTeleport[ctx.MovingPiece.team])
            {
                int pawnRow = to.y + (ctx.MovingPiece.team == GameConstants.WHITE_TEAM ? -1 : 1);
                
                if (pawnRow >= 0 && pawnRow < 8)
                {
                    ctx.EnPassant = true;
                    ctx.CapturedPiece = ChessPieces[to.x, pawnRow];
                    ctx.EnPassantSquare = new Vector2Int(to.x, pawnRow);
                    ChessPieces[to.x, pawnRow] = null;
                }
            }

            ChessPieces[from.x, from.y] = null;
            ChessPieces[to.x, to.y] = ctx.MovingPiece;
            ctx.MovingPiece.currentX = to.x;
            ctx.MovingPiece.currentY = to.y;
        }

        public void UndoTemporaryMove(MoveContext ctx)
        {
            ChessPieces[ctx.From.x, ctx.From.y] = ctx.MovingPiece;
            ctx.MovingPiece.currentX = ctx.From.x;
            ctx.MovingPiece.currentY = ctx.From.y;

            ChessPieces[ctx.To.x, ctx.To.y] = ctx.CapturedPiece;

            if (ctx.EnPassant && ctx.CapturedPiece)
            {
                ChessPieces[ctx.EnPassantSquare.x, ctx.EnPassantSquare.y] = ctx.CapturedPiece;
                ctx.CapturedPiece.currentX = ctx.EnPassantSquare.x;
                ctx.CapturedPiece.currentY = ctx.EnPassantSquare.y;
            }
        }
    }
}