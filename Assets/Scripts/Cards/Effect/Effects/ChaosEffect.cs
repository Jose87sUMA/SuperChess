
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BoardLogic;
using Cards.Runtime;
using ChessPieces;
using ChessGame;

namespace Cards.Effect.Effects
{
    public sealed class ChaosEffect : CardEffect
    {
        public override bool DeferredConfirmation => true;        public override void Resolve(CardInstance inst, CardSystem sys, Chessboard board)
        {
            var pieceInfos = new List<(ChessPiece piece, int startX, int startY)>();
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                {
                    if (board.ChessPieces[x, y])
                        pieceInfos.Add((board.ChessPieces[x, y], x, y));
                }
            }

            board.changeTurnOnMove = false;
            board.canMoveEnemyPieces = true;
            foreach (var (piece, origX, origY) in pieceInfos)
            {
                Vector2Int dest;                
                do
                {
                    int rx = Random.Range(0, GameConstants.BOARD_SIZE);
                    int ry = Random.Range(0, GameConstants.BOARD_SIZE);
                    dest = new Vector2Int(rx, ry);
                }
                while (board.ChessPieces[dest.x, dest.y]);

                board.MoveTo(origX, origY, dest.x, dest.y);
                board.SendMakeMoveMessage(new Vector2Int(origX, origY), dest);
            }

            board.changeTurnOnMove = true;  
            board.canMoveEnemyPieces = false;
            
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
