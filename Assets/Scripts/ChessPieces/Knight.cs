using System.Collections.Generic;
using BoardLogic;
using ChessGame;
using UnityEngine;

namespace ChessPieces
{
    public class Knight : ChessPiece
    {
    
        public override List<Vector2Int> GetAvailableMoves(Chessboard board, int tileCountX, int tileCountY)
        {
            List<Vector2Int> moves = new List<Vector2Int>();

            foreach (var dir in KnightDirections)
            {
                Vector2Int targetPos = new Vector2Int(currentX + dir.x, currentY + dir.y);

                if (targetPos.x < 0 || targetPos.x >= tileCountX || targetPos.y < 0 || targetPos.y >= tileCountY)
                    continue;

                ChessPiece targetPiece = board.ChessPieces[targetPos.x, targetPos.y];

                if (!targetPiece || targetPiece.team != team)
                {
                    moves.Add(targetPos);
                }
            }

            if (!board.canTeleport[team]) return moves;
            AddTeleportMoves(board, ref moves);
            
            return moves;
        }
    }
}