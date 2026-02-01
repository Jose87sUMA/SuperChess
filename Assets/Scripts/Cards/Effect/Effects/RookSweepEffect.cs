
using System;
using UnityEngine;
using BoardLogic;
using Cards.Runtime;
using ChessPieces;
using ChessGame;

namespace Cards.Effect.Effects
{
    public sealed class RookSweepEffect : CardEffect, ITileClickListener
    {
        private Rook _rook = null;
        public override bool DeferredConfirmation => true;

        public override void Resolve(CardInstance inst, CardSystem sys, Chessboard board)
        {
            Inst = inst;
            Sys = sys;
            board.dragEnabled = false;
            Sys.currentlyPlayingCard = true;
            TilePicker.Register(this);
            Sys.GetComponent<HandUI>()?.ShowInfo("Elige la torre con la que hacer el barrido");
        }

        public void OnBoardClick(Vector2Int square, Chessboard board)
        {
            ChessPiece piece = board.ChessPieces[square.x, square.y];
            if (!_rook && (!piece || piece.team != board.myTeam || piece.type != ChessPieceType.Rook))
            {
                CancelAndClean(board, "La casilla no contiene una torre aliada");
                return;
            }
            
            if (!_rook)
            {
                _rook = (Rook) piece;
                Sys.GetComponent<HandUI>()?.ShowInfo("Haz click en la dirección en la que hacer el barrido");
                return;
            }
            
            int xDiff = square.x - _rook.currentX;
            int yDiff = square.y - _rook.currentY;

            if ((xDiff != 0 && yDiff != 0) || (xDiff == 0 && yDiff == 0))
            {
                _rook = null;
                CancelAndClean(board, "Selecciona una dirección horizontal o vertical respecto a la torre");
                return;
            }
            
            board.canCaptureAlliedPieces = true;
            board.changeTurnOnMove = false;

            int dirX = xDiff == 0 ? 0 : (xDiff  > 0 ? 1 : -1);
            int dirY = yDiff == 0 ? 0 : (yDiff  > 0 ? 1 : -1);
            int nextX = _rook.currentX + dirX;
            int nextY = _rook.currentY + dirY;

            while (nextX >= GameConstants.MIN_BOARD_INDEX && nextX <= GameConstants.MAX_BOARD_INDEX && 
                   nextY >= GameConstants.MIN_BOARD_INDEX && nextY <= GameConstants.MAX_BOARD_INDEX)
            {
                int prevX = _rook.currentX;
                int prevY = _rook.currentY;
                board.MoveTo(prevX, prevY, nextX, nextY);
                board.SendMakeMoveMessage(new Vector2Int(prevX, prevY), new(nextX, nextY));

                nextX = _rook.currentX + dirX;
                nextY = _rook.currentY + dirY;
            }

            board.ChessPieces[_rook.currentX, _rook.currentY] = null;
            board.CapturePiece(_rook);
            
            Sys.SendMessage(Inst, this, true);
            TilePicker.Unregister(this);

            board.canCaptureAlliedPieces = false;
            board.changeTurnOnMove = true;
            board.dragEnabled = true;
            Sys.currentlyPlayingCard = false;

            board.FlipTurn();
            Sys.SuppressNextAutoEndTurn();
        }

        private void CancelAndClean(Chessboard board, String message)
        {
            TilePicker.Unregister(this);
            board.dragEnabled = true;
            Sys.currentlyPlayingCard = false;
            CancelCard(board, message);
            Sys.SendMessage(Inst, this, false);
        }

        public override byte[] SerializePayload()
        {
            if (!_rook)
                return System.Array.Empty<byte>();

            return new[] { (byte)_rook.currentX, (byte)_rook.currentY};
        } 

        public override void ResolveRemote(byte[] info, CardSystem cs, Chessboard board)
        {
            if (board.canCaptureAlliedPieces && info.Length > 0)
            {
                Rook rook = (Rook) board.ChessPieces[info[0], info[1]];
                board.ChessPieces[rook.currentX, rook.currentY] = null;
                board.CapturePiece(rook);
                
                board.FlipTurn();
            }
            
            board.canCaptureAlliedPieces = !board.canCaptureAlliedPieces;
            board.changeTurnOnMove = !board.changeTurnOnMove;

        }
    }
}