using System.Collections.Generic;
using BoardLogic;
using ChessGame;
using UnityEngine;

namespace ChessPieces
{
    public enum ChessPieceType
    {
        None = 0,
        Pawn = 1,
        Rook = 2,
        Knight = 3,
        Bishop = 4,
        Queen = 5,
        King = 6
    }

    public class ChessPiece : MonoBehaviour
    {

        public int team;
        public int currentX;
        public int currentY;
        public ChessPieceType type;
        public bool poisoned = false;

        private Vector3 _desiredPosition;
        private Vector3 _desiredScale = Vector3.one;
    
        protected static readonly Vector2Int[] RookDirections =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    
        protected static readonly Vector2Int[] BishopDirections =
        {
            new Vector2Int(1,  1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };
    
        protected static readonly Vector2Int[] KnightDirections =
        {
            new Vector2Int(1, 2),   // two up, one right
            new Vector2Int(2, 1),   // one up, two right
            new Vector2Int(2, -1),  // one down, two right
            new Vector2Int(1, -2),  // two down, one right
            new Vector2Int(-1, -2), // two down, one left
            new Vector2Int(-2, -1), // one down, two left
            new Vector2Int(-2, 1),  // one up, two left
            new Vector2Int(-1, 2)   // two up, one left
        };
    
        private void Start()
        {
            if (type == ChessPieceType.Knight || type == ChessPieceType.Bishop)
                transform.rotation = Quaternion.Euler(0, team == 0 ? 270: 90, 0);
        }
    
        public void Update()
        {
            transform.position = Vector3.Lerp(transform.position, _desiredPosition, Time.deltaTime * 10);
            transform.localScale = Vector3.Lerp(transform.localScale, _desiredScale, Time.deltaTime * 10);
        }
    
        public virtual void SetPosition(Vector3 position, bool force = false)
        {
            _desiredPosition = position;
            if(force)
                transform.position = _desiredPosition;
        }
    
        public virtual void SetScale(Vector3 scale, bool force = false)
        {
            _desiredScale = scale;
            if(force)
                transform.localScale = _desiredScale;
        }

        public virtual List<Vector2Int> GetAvailableMoves(Chessboard board, int tileCountX, int tileCountY)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            return moves;
        }

        public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
        {
            return SpecialMove.None;
        }        protected static void AddTeleportMoves(Chessboard board, ref List<Vector2Int> moves)
        {
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                {
                    if (!board.ChessPieces[x, y])
                        moves.Add(new Vector2Int(x, y));
                }
            }
        }



    }
}