
using System;
using UnityEngine;
using BoardLogic;
using Cards.Runtime;
using ChessPieces;
using ChessGame;
using Net;
using Net.NetMessages;
using Unity.VisualScripting;
using Object = UnityEngine.Object;

namespace Cards.Effect.Effects
{
    public sealed class AriseEffect : CardEffect, ITileClickListener
    {
        private int _xPosition = -1;
        public override bool DeferredConfirmation => true;

        public override void Resolve(CardInstance inst, CardSystem sys, Chessboard board)
        {
            Inst = inst;
            Sys = sys;

            TilePicker.Register(this);
            board.dragEnabled = false;
            Sys.currentlyPlayingCard = true;


            sys.GetComponent<HandUI>()?.ShowInfo("Elige la columna donde revivir el peón");
        }        public void OnBoardClick(Vector2Int square, Chessboard board)
        {
            Vector2Int destinationSquare = new Vector2Int(square.x, board.myTeam == GameConstants.WHITE_TEAM ? GameConstants.WHITE_PAWN_START_RANK : GameConstants.BLACK_PAWN_START_RANK);
            
            if (!board.IsSquareEmpty(destinationSquare))
            {
                CancelAndClean(board, "La casilla destino no está vacía");
                return;
            }

            var newPawn = CreateNewPawn(board, destinationSquare, board.myTeam);

            if (board.MoveLeavesKingInCheck(destinationSquare, destinationSquare, board.currentTurn))
            {
                board.ChessPieces[destinationSquare.x, destinationSquare.y] = null;
                Object.Destroy(newPawn.gameObject);
                CancelAndClean(board, "Renacimiento dejaría a tu rey en jaque");
                return;
            }
            
            _xPosition = destinationSquare.x;
            Sys.SendMessage(Inst, this, true);
            board.FlipTurn();
            
            TilePicker.Unregister(this);
            board.dragEnabled = true;
            Sys.currentlyPlayingCard = false;
            Sys.SuppressNextAutoEndTurn();
        }

        private void CancelAndClean(Chessboard board, String message)
        {
            TilePicker.Unregister(this);
            board.dragEnabled = true;
            Sys.currentlyPlayingCard = false;
            CancelCard(board, message);
        }

        private ChessPiece CreateNewPawn(Chessboard board, Vector2Int destinationSquare, int team)
        {
            ChessPiece newPawn = board.SpawnSinglePiece(ChessPieceType.Pawn, team);
            newPawn.currentX = destinationSquare.x;
            newPawn.currentY = destinationSquare.y;
            board.ChessPieces[destinationSquare.x, destinationSquare.y] = newPawn;
            newPawn.SetPosition(board.GetPosition(destinationSquare.x, destinationSquare.y), true);
            return newPawn;
        }

        public override byte[] SerializePayload()
        {
            return _xPosition < 0 ? System.Array.Empty<byte>() : new [] { (byte)_xPosition };
        }

        public override void ResolveRemote(byte[] info, CardSystem cs, Chessboard board)
        {
            if (info == null || info.Length == 0)
                return;
            
            int x = info[0];
            int y = board.myTeam == 0 ? 6 : 1;
            var dest = new Vector2Int(x, y);

            if (!board.IsSquareEmpty(dest))
                return;

            CreateNewPawn(board, dest, board.myTeam == 0 ? 1 : 0);
            board.FlipTurn();
        }
    }
}