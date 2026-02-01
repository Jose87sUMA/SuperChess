using System.Collections.Generic;
using BoardLogic;
using ChessGame;
using UnityEngine;

namespace ChessPieces
{
    public class King : ChessPiece
    {

        public override List<Vector2Int> GetAvailableMoves(Chessboard board, int tileCountX, int tileCountY)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
        
            foreach (var dir in RookDirections)
            {
                int x = currentX + dir.x;
                int y = currentY + dir.y;

                if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY)
                {
                    ChessPiece piece = board.ChessPieces[x, y];

                    if (!piece || piece.team != team)            
                    {
                        moves.Add(new Vector2Int(x, y));
                    }
                }
            }
        
            foreach (var dir in BishopDirections)
            {
                int x = currentX + dir.x;
                int y = currentY + dir.y;

                if (x >= 0 && x < tileCountX &&
                    y >= 0 && y < tileCountY)
                {
                    ChessPiece piece = board.ChessPieces[x, y];

                    if (!piece || piece.team != team)
                    {
                        moves.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (!board.canTeleport[team]) return moves;
            AddTeleportMoves(board, ref moves);
            
            return moves;
        }

        public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
        {
            SpecialMove specialMove = SpecialMove.None;
            int rank = (team == 0 ? 0 : 7);
        
            bool kingMoved = moveList.Find(m => m[0].x == 4 && m[0].y == rank) != null;            bool leftRookMoved = moveList.Find(m => m[0].x == GameConstants.MIN_BOARD_INDEX && m[0].y == rank) != null;
            bool rightRookMoved = moveList.Find(m => m[0].x == GameConstants.MAX_BOARD_INDEX && m[0].y == rank) != null;

            if (kingMoved)
                return specialMove;

            ChessPiece pieceAtLeftCorner = board[GameConstants.MIN_BOARD_INDEX, rank];
            ChessPiece pieceAtRightCorner = board[GameConstants.MAX_BOARD_INDEX, rank];

            if (!pieceAtLeftCorner && !pieceAtRightCorner)
                return specialMove;
            
            if (pieceAtLeftCorner&& !leftRookMoved && pieceAtLeftCorner.type == ChessPieceType.Rook && pieceAtLeftCorner.team == team)
            {
                bool freeSpace = true;
                int i = 1;
                while (freeSpace && i < 4)
                {
                    freeSpace = !board[i, rank];
                    ++i;
                }

                if (freeSpace)
                {
                    availableMoves.Add(new Vector2Int(2, rank));
                    specialMove = SpecialMove.Castle;
                }
            }
        
            if (pieceAtRightCorner && !rightRookMoved && pieceAtRightCorner.type == ChessPieceType.Rook && pieceAtRightCorner.team == team)
            {
                bool freeSpace = true;
                int i = 5;
                while (freeSpace && i < 7)
                {
                    freeSpace = !board[i, rank];
                    ++i;
                }

                if (freeSpace)
                {
                    availableMoves.Add(new Vector2Int(6, rank));
                    specialMove = SpecialMove.Castle;
                }
            }
        
            return specialMove;

        }
    }
}
