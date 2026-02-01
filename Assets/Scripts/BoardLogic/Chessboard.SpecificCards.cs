using System;

namespace BoardLogic
{
    public partial class Chessboard
    {
        //01-MagicStorm
        int  _globalMoveCap = -1;
        int  _capTurnsLeft = 0;

        public void SetGlobalMoveCap(int maxSquares, int turns)
        {
            _globalMoveCap = maxSquares;
            _capTurnsLeft  = turns;
        }

        void TickGlobalMoveCap()
        {
            if (_capTurnsLeft <= 0) 
                return;
            
            _capTurnsLeft--;
            if (_capTurnsLeft == 0) 
                _globalMoveCap = -1;
        }

        public bool IsMoveDistanceAllowed(int dx, int dy) => _globalMoveCap < 0 || (Math.Max(Math.Abs(dx), Math.Abs(dy)) <= _globalMoveCap);
        
        //00-Teleport
        public bool[] canTeleport = new bool[2];
        
        // 02-StrategicJump
        public bool[] canJump = new bool[2];
        
        // 04-Chaos
        public bool canMoveEnemyPieces = false;
        
        //08-TowerSweep
        public bool canCaptureAlliedPieces = false;
    }
}