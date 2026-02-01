using System;
using System.Collections.Generic;
using System.Linq;
using ChessPieces;
using ChessGame;
using UnityEngine;

namespace BoardLogic
{
    public partial class Chessboard
    {
        public void MoveTo(int originalX, int originalY, int destinationX, int destinationY)
        {
            Vector2Int previousPosition = new Vector2Int(originalX, originalY);
            ChessPiece piece = ChessPieces[originalX, originalY];

            var target = ChessPieces[destinationX, destinationY];
            
            if (target && (!canCaptureAlliedPieces && target.team == piece.team))
                return;

            bool targetWasPoisoned = false;
            if (target)
            {
                targetWasPoisoned = target.poisoned;
                CapturePiece(target);
            }

            if (targetWasPoisoned)
            {
                CapturePiece(piece);
                ChessPieces[destinationX, destinationY] = null;
            }
            else
            {
                ChessPieces[destinationX, destinationY] = piece;
                PositionSinglePiece(destinationX, destinationY);
            }
            
            ChessPieces[previousPosition.x, previousPosition.y] = null;
         
            MoveList.Add(new [] {previousPosition, new Vector2Int(destinationX, destinationY)});
            
            ToggleHighlights(false);
            ProcessSpecialMove();
            
            OnPieceMoved?.Invoke(piece);
            if (_retainTurnOnce)
            {
                _retainTurnOnce = false;      
                return;                     
            }
            
            if(changeTurnOnMove)
            {
                FlipTurn();
                CheckForEndGame();
                RaiseTurnEnded();
            }
            
        }

        public void FlipTurn()
        {
            currentTurn = 1 - currentTurn;

            if (localGame)
            {
                myTeam = 1 - myTeam;
                if (switchCameraOnPlay)
                    MoveCameraToCurrentTeam();
            }

            RaiseTurnStarted();
        }

        public void CapturePiece(ChessPiece otherPiece)
        {
            if (otherPiece.team == GameConstants.WHITE_TEAM)
            {
                _deadWhites.Add(otherPiece);
                otherPiece.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) +
                                       _deadWhites.Count * deathSpacing * Vector3.forward);
            }
            else
            {
                _deadBlacks.Add(otherPiece);
                otherPiece.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize) +
                                       _deadBlacks.Count * deathSpacing * Vector3.back);
            }

            if (otherPiece.type == ChessPieceType.King)
            {
                
                DisplayVictory(1 - otherPiece.team);
            }
            otherPiece.SetScale(Vector3.one * deathSize);
        }
        
        public List<Vector2Int> FilteredMoves(ChessPiece piece)
        {
            var moves = piece.GetAvailableMoves(this, GameConstants.BOARD_SIZE, GameConstants.BOARD_SIZE);

            if (_globalMoveCap < 0) return moves;

            int px = piece.currentX;
            int py = piece.currentY;
            
            return moves
                .Where(m => Math.Max(Math.Abs(m.x - px), Math.Abs(m.y - py)) <= _globalMoveCap)
                .ToList();
        }

        private static bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
        {
            return moves.Any(move => move.x == pos.x && move.y == pos.y);
        }

        // Special Moves
        private void ProcessSpecialMove()
        {
            Vector2Int movedPiecePosition = MoveList[^1][1];

            switch (specialMove)
            {
                case (SpecialMove.EnPassant):
                    Vector2Int targetPawnPosition = MoveList[^2][1];

                    if (targetPawnPosition.x == movedPiecePosition.x &&
                        Math.Abs(targetPawnPosition.y - movedPiecePosition.y) == 1)
                    {
                        CapturePiece(ChessPieces[targetPawnPosition.x, targetPawnPosition.y]);
                        ChessPieces[targetPawnPosition.x, targetPawnPosition.y] = null;
                    }
                    break;

                case (SpecialMove.Promotion):
                    ChessPiece movedPawn = ChessPieces[movedPiecePosition.x, movedPiecePosition.y];
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, movedPawn.team);
                    ChessPieces[movedPiecePosition.x, movedPiecePosition.y] = newQueen;
                    Destroy(movedPawn.gameObject);
                    PositionSinglePiece(movedPiecePosition.x, movedPiecePosition.y, true);
                    break;

                case (SpecialMove.Castle):
                    Vector2Int kingPreviousPositon = MoveList[^1][0];
                    if (Math.Abs(kingPreviousPositon.x - movedPiecePosition.x) == 2)
                        Castle(kingPreviousPositon, movedPiecePosition);
                    break;
            }
            specialMove = SpecialMove.None;
        }        private void Castle(Vector2Int kingFrom, Vector2Int kingTo)
        {
            int rookSrcX   = kingTo.x > kingFrom.x ? GameConstants.MAX_BOARD_INDEX : GameConstants.MIN_BOARD_INDEX;
            int rookDestX  = kingTo.x > kingFrom.x ? kingTo.x - 1 : kingTo.x + 1;

            ChessPiece rook = ChessPieces[rookSrcX, kingFrom.y];
            ChessPieces[rookSrcX, kingFrom.y] = null;
            ChessPieces[rookDestX, kingFrom.y] = rook;
            PositionSinglePiece(rookDestX, kingFrom.y);
        }
    }
}