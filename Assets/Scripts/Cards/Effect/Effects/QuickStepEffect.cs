
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BoardLogic;
using Cards.Runtime;
using ChessPieces;
using ChessGame;

namespace Cards.Effect.Effects
{
    public sealed class QuickStepEffect : CardEffect
    {
        public override bool DeferredConfirmation => true;        public override void Resolve(CardInstance inst, CardSystem sys, Chessboard board)
        {
            List<Pawn> alliedPawns = new List<Pawn>();

            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
            {
                ChessPiece piece = board.ChessPieces[x, y];
                if (piece && piece.type == ChessPieceType.Pawn && piece.team == board.myTeam)
                    alliedPawns.Add((Pawn) piece);
            }

            if (alliedPawns.Count == 0)
            {
                sys.RefundCard(inst);
                sys.GetComponent<HandUI>()?.ShowInfo("No pawns to move");
                return;
            }

            board.changeTurnOnMove = false;            foreach (Pawn pawn in alliedPawns)
            {
                List<Vector2Int> moves = pawn.GetAvailableMoves(board, GameConstants.BOARD_SIZE, GameConstants.BOARD_SIZE);

                int direction = pawn.team == GameConstants.WHITE_TEAM ? +1 : -1;
                int startX    = pawn.currentX;
                int startY    = pawn.currentY;

                var forwardMoves = moves
                    .Where(m => m.x == startX)
                    .ToList();

                if (forwardMoves.Count > 0)
                {
                    var selectedMove = forwardMoves
                        .OrderByDescending(m => (m.y - startY) * direction)
                        .First();

                    List<Vector2Int> allMoves = board.FilteredMoves(pawn);
                    board.specialMove = pawn.GetSpecialMoves(ref board.ChessPieces, ref board.MoveList, ref allMoves);
                    board.MoveTo(startX, startY, selectedMove.x, selectedMove.y);
                    board.SendMakeMoveMessage(new Vector2Int(startX, startY), new Vector2Int(selectedMove.x, selectedMove.y));
                }
            }
            
            board.changeTurnOnMove = true;
            board.FlipTurn();
            
            sys.SendMessage(inst, this, true);
        }
        
        public override void ResolveRemote(byte[] _, CardSystem cs, Chessboard board)
        {
            board.changeTurnOnMove = !board.changeTurnOnMove;
            
            if (board.changeTurnOnMove)
                board.FlipTurn();
        }
    }
}
