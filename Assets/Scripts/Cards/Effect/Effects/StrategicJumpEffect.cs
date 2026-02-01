
using System;
using UnityEngine;
using BoardLogic;
using Cards.Runtime;
using ChessPieces;
using Net;
using Net.NetMessages;

namespace Cards.Effect.Effects
{
    public sealed class StrategicJumpEffect : CardEffect
    {
        private Chessboard  _board;
        private int _team;
        
        public override void Resolve(CardInstance inst, CardSystem sys, Chessboard board)
        {
            _board = board;
            _team = board.myTeam;
            board.canJump[_team] = true;
            board.TurnEnded += OnTurnEnded;
        }

        private void OnTurnEnded(int team)
        {
            _board.canJump[_team] = false;
            _board.TurnEnded -= OnTurnEnded;
        }
    }
}
