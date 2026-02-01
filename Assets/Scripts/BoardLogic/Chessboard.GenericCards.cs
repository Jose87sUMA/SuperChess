using System;
using Cards.Runtime;
using ChessPieces;
using UnityEngine;
namespace BoardLogic
{
    public partial class Chessboard
    {
        public event Action<ChessPiece> OnPieceMoved;
        [SerializeField] private CardSystem cardSystem;

        public event Action<int> TurnStarted;
        public event Action<int> TurnEnded;

        public bool changeTurnOnMove = true;
        public bool dragEnabled = true;
        
        private int _turnCounter;              

        private void RaiseTurnStarted()
        {
            TurnStarted?.Invoke(currentTurn);
        }

        private void RaiseTurnEnded()
        {
            TurnEnded?.Invoke(currentTurn);
            _turnCounter++;                    
            TickGlobalMoveCap();
        }
    }
}