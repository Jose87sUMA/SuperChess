using System;
using System.Collections.Generic;
using BoardLogic;
using ChessPieces;
using ChessGame;
using UnityEngine;

namespace AI
{
    public static class BoardHelper
    {
        public static void ForEachPiece(ChessPiece[,] pieces, Action<ChessPiece, int, int> action)
        {
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                {
                    var piece = pieces[x, y];
                    if (piece != null)
                    {
                        action(piece, x, y);
                    }
                }
            }
        }
        
        public static void ForEachPieceOfTeam(ChessPiece[,] pieces, int team, Action<ChessPiece, int, int> action)
        {
            ForEachPiece(pieces, (piece, x, y) =>
            {
                if (piece.team == team)
                {
                    action(piece, x, y);
                }
            });
        }
        
        public static void ForEachPieceOfType(ChessPiece[,] pieces, ChessPieceType type, Action<ChessPiece, int, int> action)
        {
            ForEachPiece(pieces, (piece, x, y) =>
            {
                if (piece.type == type)
                {
                    action(piece, x, y);
                }
            });
        }        
        public static Vector2Int FindPiece(ChessPiece[,] pieces, Func<ChessPiece, bool> predicate)
        {
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                {
                    var piece = pieces[x, y];
                    if (piece != null && predicate(piece))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            return new Vector2Int(-1, -1);
        }
        
        public static bool IsValidPosition(int x, int y)
        {
            return GameUtils.IsValidPosition(x, y);
        }
    }
}
