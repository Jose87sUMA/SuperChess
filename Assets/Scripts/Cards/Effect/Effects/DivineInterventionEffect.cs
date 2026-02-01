
using System;
using UnityEngine;
using BoardLogic;
using Cards.Runtime;
using ChessPieces;
using ChessGame;

namespace Cards.Effect.Effects
{
    public sealed class DivineInterventionEffect : CardEffect, ITileClickListener
    {
        private Bishop _bishop = null;
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
            if (!_bishop && (!piece || piece.team != board.myTeam || piece.type != ChessPieceType.Bishop))
            {
                CancelAndClean(board, "La casilla no contiene un alfil aliado");
                return;
            }
            
            if (!_bishop)
            {
                _bishop = (Bishop) piece;
                Sys.GetComponent<HandUI>()?.ShowInfo("Haz click en la dirección en la que hacer la intervención");
                return;
            }
            
            int xDiff = square.x - _bishop.currentX;
            int yDiff = square.y - _bishop.currentY;

            if ((xDiff == 0 && yDiff == 0) || Math.Abs(xDiff) != Math.Abs(yDiff))
            {
                _bishop = null;
                CancelAndClean(board, "Selecciona una dirección diagonal respecto al alfil");
                return;
            }
            
            board.canCaptureAlliedPieces = true;
            board.changeTurnOnMove = false;

            int dirX = xDiff >  0 ?  1 : -1;
            int dirY = yDiff >  0 ?  1 : -1;
            int nextX = _bishop.currentX + dirX;
            int nextY = _bishop.currentY + dirY;

            while (nextX >= GameConstants.MIN_BOARD_INDEX && nextX <= GameConstants.MAX_BOARD_INDEX && 
                   nextY >= GameConstants.MIN_BOARD_INDEX && nextY <= GameConstants.MAX_BOARD_INDEX)
            {
                int prevX = _bishop.currentX;
                int prevY = _bishop.currentY;
                board.MoveTo(prevX, prevY, nextX, nextY);
                board.SendMakeMoveMessage(new Vector2Int(prevX, prevY), new(nextX, nextY));

                nextX = _bishop.currentX + dirX;
                nextY = _bishop.currentY + dirY;
            }

            board.ChessPieces[_bishop.currentX, _bishop.currentY] = null;
            board.CapturePiece(_bishop);

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
            if (!_bishop)
                return System.Array.Empty<byte>();

            return new[] { (byte)_bishop.currentX, (byte)_bishop.currentY};
        } 

        public override void ResolveRemote(byte[] info, CardSystem cs, Chessboard board)
        {
            if (board.canCaptureAlliedPieces && info.Length > 0)
            {
                Bishop bishop = (Bishop) board.ChessPieces[info[0], info[1]];
                board.ChessPieces[bishop.currentX, bishop.currentY] = null;
                board.CapturePiece(bishop);
                
                board.FlipTurn();
            }
            
            board.canCaptureAlliedPieces = !board.canCaptureAlliedPieces;
            board.changeTurnOnMove = !board.changeTurnOnMove;

        }
    }
}