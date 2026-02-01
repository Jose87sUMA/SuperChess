
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BoardLogic;
using Cards.Runtime;
using ChessPieces;
using ChessGame;

namespace Cards.Effect.Effects
{
    public sealed class UncoodinatedAttackEffect : CardEffect
    {
        public override bool DeferredConfirmation => true;        public override void Resolve(CardInstance inst, CardSystem sys, Chessboard board)
        {
            var pieces = new List<ChessPiece>();
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                {
                    ChessPiece piece = board.ChessPieces[x, y];
                    if (piece && piece.team == board.myTeam)
                        pieces.Add(piece);
                }
            }

            board.changeTurnOnMove = false;
            foreach (var piece in pieces)
            {
                List<Vector2Int> moves = board.FilteredMoves(piece);
                if (moves.Count == 0)
                    continue;            

                Vector2Int selected = moves[Random.Range(0, moves.Count)];

                int origX = piece.currentX;
                int origY = piece.currentY;
                board.specialMove = piece.GetSpecialMoves(ref board.ChessPieces, ref board.MoveList, ref moves);
                board.MoveTo(origX, origY, selected.x, selected.y);
                board.SendMakeMoveMessage(new Vector2Int(origX, origY), selected);
            }

            board.changeTurnOnMove = true;  
            
            board.FlipTurn();
            board.CheckForEndGame();
            sys.SendMessage(inst, this, true);
        }
        
        public override void ResolveRemote(byte[] _, CardSystem cs, Chessboard board)
        {
            board.changeTurnOnMove = !board.changeTurnOnMove;

            if (board.changeTurnOnMove)
            {
                board.FlipTurn();
                board.CheckForEndGame();
            }
        }
    }
}
