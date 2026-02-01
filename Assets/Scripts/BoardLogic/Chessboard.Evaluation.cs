using System.Collections.Generic;
using System.Linq;
using ChessGame;
using ChessPieces;
using UnityEngine;

namespace BoardLogic
{
    public partial class Chessboard
    {
        public bool check = false;
        public bool checkmate = false;
        
        public void PreventCheck(ChessPiece piece)
        {
            ChessPiece targetKing = null;
        
            for (int x = 0; x < GameConstants.BOARD_SIZE; ++x)
            for (int y = 0; y < GameConstants.BOARD_SIZE; ++y)
                if (ChessPieces[x, y] && ChessPieces[x, y].team == piece.team && ChessPieces[x, y].type == ChessPieceType.King)
                    targetKing = ChessPieces[x, y];
            
            SimulateMoveForSinglePiece(piece, targetKing);
        }
    
        private void SimulateMoveForSinglePiece(ChessPiece currentPiece, ChessPiece targetKing)
        {
            int startX = currentPiece.currentX;
            int startY = currentPiece.currentY;

            ChessPiece kingToGuard = targetKing;

            List<Vector2Int> movesToRemove = new();

            foreach (Vector2Int move in _availableMoves)
            {
                ChessPiece capturedPiece;
                bool enPassant = false;

                if (currentPiece.type == ChessPieceType.Pawn &&
                    !ChessPieces[move.x, move.y] &&
                    move.x != startX && !canTeleport[currentTurn])
                {
                    enPassant = true;
                    int pawnRow = move.y + (currentPiece.team == GameConstants.WHITE_TEAM ? -1 : 1);
                    capturedPiece = ChessPieces[move.x, pawnRow];
                    ChessPieces[move.x, pawnRow] = null;
                }
                else
                {
                    capturedPiece = ChessPieces[move.x, move.y];
                }

                ChessPieces[startX, startY]    = null;
                ChessPieces[move.x, move.y]    = currentPiece;
                currentPiece.currentX          = move.x;
                currentPiece.currentY          = move.y;

                if (currentPiece.type == ChessPieceType.King)
                    kingToGuard = currentPiece;

                bool kingIsInCheck = false;

                for (int x = 0; x < GameConstants.BOARD_SIZE && !kingIsInCheck; ++x)
                for (int y = 0; y < GameConstants.BOARD_SIZE && !kingIsInCheck; ++y)
                {
                    ChessPiece enemy = ChessPieces[x, y];
                    if (enemy is null || enemy.team == currentPiece.team) continue;

                    List<Vector2Int> enemyMoves = FilteredMoves(enemy);
                    if (enemyMoves.Any(enemyMove => enemyMove.x == kingToGuard.currentX && enemyMove.y == kingToGuard.currentY))
                    {
                        kingIsInCheck = true;
                    }
                }

                ChessPieces[startX, startY] = currentPiece;
                ChessPieces[move.x, move.y] = capturedPiece;

                if (enPassant && capturedPiece)
                {
                    int pawnRow = move.y + (currentPiece.team == GameConstants.WHITE_TEAM ? -1 : 1);
                    ChessPieces[move.x, pawnRow] = capturedPiece;
                    capturedPiece.currentX       = move.x;
                    capturedPiece.currentY       = pawnRow;
                }

                currentPiece.currentX = startX;
                currentPiece.currentY = startY;

                if (kingIsInCheck)
                    movesToRemove.Add(move);
            }

            foreach (Vector2Int bad in movesToRemove)
                _availableMoves.Remove(bad);
        }
    
        public void EvaluateBoardState(int team, out bool kingInCheck, out bool hasLegalMove)
        {
            kingInCheck = false;
            hasLegalMove = false;

            ChessPiece king = null;

            for (int x = 0; x < GameConstants.BOARD_SIZE && !king; ++x)
            for (int y = 0; y < GameConstants.BOARD_SIZE && !king; ++y)
                if (ChessPieces[x, y] && ChessPieces[x, y].team == team && ChessPieces[x, y].type == ChessPieceType.King)
                    king = ChessPieces[x, y];
            
            if (!king)
                return;

            for (int x = 0; x < GameConstants.BOARD_SIZE; ++x)
            for (int y = 0; y < GameConstants.BOARD_SIZE; ++y)
            {
                ChessPiece piece = ChessPieces[x, y];
                if (!piece || piece.team != team) continue;

                List<Vector2Int> moves = FilteredMoves(piece);

                _availableMoves = moves;
                PreventCheck(piece);

                if (_availableMoves.Count > 0)
                    hasLegalMove = true;
            }

            for (int x = 0; x < GameConstants.BOARD_SIZE && !kingInCheck; ++x)
            for (int y = 0; y < GameConstants.BOARD_SIZE && !kingInCheck; ++y)
            {
                ChessPiece enemy = ChessPieces[x, y];
                if (!enemy || enemy.team == team) continue;

                List<Vector2Int> enemyMoves = FilteredMoves(enemy);

                if (enemyMoves.Any(m => m.x == king.currentX && m.y == king.currentY))
                    kingInCheck = true;
            }
        
            _availableMoves.Clear();
            _currentlyDragging = null;   
        }
        public void CheckForEndGame()
        {
            int sideToMove = currentTurn;

            if (HasInsufficientMaterial())
            {
                DisplayVictory(GameConstants.GAME_DRAW);
                return;
            }

            EvaluateBoardState(sideToMove, out bool kingInCheck, out bool hasMove);

            if (hasMove)
            {
                if (kingInCheck)
                {
                    check = true;
                }
                else
                {
                    check = false;
                }
                return;
            }

            if (kingInCheck)
            {
                int winner = (sideToMove == GameConstants.WHITE_TEAM) ? GameConstants.BLACK_TEAM : GameConstants.WHITE_TEAM;
                checkmate = true;
                DisplayVictory(winner);
            }
            else
            {
                victoryScreen.SetActive(true);
            }
        }
        

        private bool HasInsufficientMaterial()
        {
            var whitePieces = new List<ChessPieceType>();
            var blackPieces = new List<ChessPieceType>();
            
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                {
                    var piece = ChessPieces[x, y];
                    if (piece == null) continue;
                    
                    if (piece.team == GameConstants.WHITE_TEAM)
                    {
                        if (piece.type != ChessPieceType.King)
                            whitePieces.Add(piece.type);
                    }
                    else
                    {
                        if (piece.type != ChessPieceType.King)
                            blackPieces.Add(piece.type);
                    }
                }
            }
            
            return IsInsufficientMaterial(whitePieces) && IsInsufficientMaterial(blackPieces);
        }
        
        private bool IsInsufficientMaterial(List<ChessPieceType> pieces)
        {
            if (pieces.Count == 0)
                return true;
                
            if (pieces.Count == 1)
            {
                var piece = pieces[0];
                return piece == ChessPieceType.Knight || piece == ChessPieceType.Bishop;
            }
            
            if (pieces.Count == 2)
            {
                return pieces.All(p => p == ChessPieceType.Knight);
            }
            
            if (pieces.Any(p => p == ChessPieceType.Pawn || p == ChessPieceType.Queen || p == ChessPieceType.Rook))
                return false;
                
            return false;
        }
        
        public bool MoveLeavesKingInCheck(Vector2Int from, Vector2Int to, int team)
        {
            ChessPiece a = ChessPieces[from.x, from.y];
            ChessPiece b = ChessPieces[to.x,   to.y];

            ChessPieces[to.x,   to.y] = a;
            ChessPieces[from.x, from.y] = null;

            int oldX = a.currentX, oldY = a.currentY;
            a.currentX = to.x;  a.currentY = to.y;

            EvaluateBoardState(team, out bool kingInCheck, out _);

            ChessPieces[from.x, from.y] = a;
            ChessPieces[to.x,   to.y]   = b;

            a.currentX = oldX;  a.currentY = oldY;

            return kingInCheck;
        }
    }
}