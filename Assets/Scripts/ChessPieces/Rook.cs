using System.Collections.Generic;
using BoardLogic;
using ChessGame;
using UnityEngine;

namespace ChessPieces
{
    public class Rook : ChessPiece
    {
        public override List<Vector2Int> GetAvailableMoves(Chessboard board, int tileCountX, int tileCountY)
        {
            List<Vector2Int> moves = new List<Vector2Int>();

            foreach (var dir in RookDirections)
            {
                int x = currentX + dir.x;
                int y = currentY + dir.y;
                bool jumpConsumed = false;

                while (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY)
                {
                    ChessPiece piece = board.ChessPieces[x, y];

                    if (!piece)            
                    {
                        moves.Add(new Vector2Int(x, y));
                    }
                    else
                    {
                        if (piece.team != team)  
                            moves.Add(new Vector2Int(x, y));
                        
                        if (board.canJump[team] && !jumpConsumed)
                            jumpConsumed = true;
                        else
                            break;
                    }
                    x += dir.x;
                    y += dir.y;
                }
            }
            
            if (!board.canTeleport[team]) return moves;
            AddTeleportMoves(board, ref moves);
            
            return moves;
        }
    }
}