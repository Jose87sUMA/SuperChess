
using System;
using UnityEngine;
using BoardLogic;
using Cards.Runtime;
using ChessPieces;

namespace Cards.Effect.Effects
{
    public sealed class PoisonedPieceEffect : CardEffect, ITileClickListener
    {
        private Vector2Int _position = new(-1, -1);
        public override bool DeferredConfirmation => true;

        public override void Resolve(CardInstance inst, CardSystem sys, Chessboard board)   
        {
            Inst = inst;
            Sys = sys;

            TilePicker.Register(this);
            board.dragEnabled = false;
            Sys.currentlyPlayingCard = true;
            Sys.GetComponent<HandUI>()?.ShowInfo("Elige la pieza a envenenar");
        }

        public void OnBoardClick(Vector2Int square, Chessboard board)
        {
            ChessPiece piece = board.ChessPieces[square.x, square.y];
            if (!piece || piece.team != board.myTeam)
            {
                CancelAndClean(board, "La casilla no contiene una pieza aliada");
                return;
            }
            
            piece.poisoned = true;
            _position = square;
            
            
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

        public override byte[] SerializePayload()
        {
            if (_position.x < 0)
                return System.Array.Empty<byte>();

            return new[] { (byte)_position.x, (byte)_position.y };
        }

        public override void ResolveRemote(byte[] info, CardSystem cs, Chessboard board)
        {
            if (info == null || info.Length < 2)
                return;

            int x = info[0];
            int y = info[1];

            ChessPiece piece = board.ChessPieces[x, y];
            if (!piece)
                return;
            
            piece.poisoned = true; 
            board.FlipTurn();
        }
    }
}