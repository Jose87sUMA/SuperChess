using System;
using System.Collections.Generic;
using BoardLogic;
using ChessGame;
using UnityEngine;

namespace ChessPieces
{
    public class Pawn : ChessPiece
    {        public override List<Vector2Int> GetAvailableMoves(Chessboard board, int tileCountX, int tileCountY)
        {
            List<Vector2Int> moves = new List<Vector2Int>();

            int direction = (team == GameConstants.WHITE_TEAM) ? 1 : -1;
            
            bool isInsideBoundsY = 0 <= currentY + direction && currentY + direction < tileCountY;
            bool isFirstMove = (team == GameConstants.WHITE_TEAM && currentY == GameConstants.WHITE_PAWN_START_RANK) || 
                              (team == GameConstants.BLACK_TEAM && currentY == GameConstants.BLACK_PAWN_START_RANK);

            if (isInsideBoundsY && !board.ChessPieces[currentX, currentY + direction])
                moves.Add(new Vector2Int(currentX, currentY + direction));
            
            if (isFirstMove && (!board.ChessPieces[currentX, currentY + direction] || board.canJump[team]) && !board.ChessPieces[currentX, currentY + 2*direction])
                moves.Add(new Vector2Int(currentX, currentY + 2 * direction));

            if (currentX + 1 < tileCountX && isInsideBoundsY)
            {
                ChessPiece pieceToCapture = board.ChessPieces[currentX + 1, currentY + direction];
                if (pieceToCapture&& pieceToCapture.team != team)
                    moves.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        
            if (0 <= currentX - 1 && isInsideBoundsY)
            {
                ChessPiece pieceToCapture = board.ChessPieces[currentX - 1, currentY + direction];
                if (pieceToCapture&& pieceToCapture.team != team)
                    moves.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
            
            if (!board.canTeleport[team]) return moves;
            AddTeleportMoves(board, ref moves);
            
            return moves;
        }

        public override SpecialMove GetSpecialMoves(
            ref ChessPiece[,] board,
            ref List<Vector2Int[]> moveList,
            ref List<Vector2Int> availableMoves)
        {
        
            int direction = team == 0 ? 1 : -1;

            if ((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
                return SpecialMove.Promotion;
        
            if (moveList.Count == 0) return SpecialMove.None;

            Vector2Int[] lastMove   = moveList[^1];
            Vector2Int   fromSquare = lastMove[0];
            Vector2Int   toSquare   = lastMove[1];

            ChessPiece movedPiece = board[toSquare.x, toSquare.y];
            
            if (!movedPiece)
                return SpecialMove.None;

            bool isOpponentPawn = movedPiece.type == ChessPieceType.Pawn && movedPiece.team != team;
            bool isDoublePush   = Math.Abs(fromSquare.y - toSquare.y) == 2;
            bool isOnSameRank   = toSquare.y == currentY;

            if (!(isOpponentPawn && isDoublePush && isOnSameRank))
                return SpecialMove.None;

            int dx = toSquare.x - currentX;
            if (Math.Abs(dx) != 1) return SpecialMove.None;

            availableMoves.Add(new Vector2Int(currentX + dx, currentY + direction));
            return SpecialMove.EnPassant;
        }

    }
}
