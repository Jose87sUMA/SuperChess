using ChessPieces;
using ChessGame;
using UnityEngine;

namespace AI
{
    public static class EvaluationHelper
    {
        public static bool IsPassedPawn(ChessPiece[,] pieces, int x, int y, int team)
        {
            var piece = pieces[x, y];
            if (piece == null || piece.type != ChessPieceType.Pawn || piece.team != team)
                return false;
                
            int direction = GameUtils.GetForwardDirection(team);
            int enemyTeam = GameUtils.GetOppositeTeam(team);
            
            for (int checkY = y + direction; 
                 checkY >= 0 && checkY < GameConstants.BOARD_SIZE; 
                 checkY += direction)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int checkX = x + dx;
                    if (GameUtils.IsValidPosition(checkX, checkY))
                    {
                        var blockingPiece = pieces[checkX, checkY];
                        if (blockingPiece != null && 
                            blockingPiece.type == ChessPieceType.Pawn && 
                            blockingPiece.team == enemyTeam)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        

        public static bool HasPawnSupport(ChessPiece[,] pieces, int x, int y, int team)
        {
            for (int dx = -1; dx <= 1; dx += 2)
            {
                if (GameUtils.IsValidPosition(x + dx, y))
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (GameUtils.IsValidPosition(x + dx, y + dy))
                        {
                            var supportPiece = pieces[x + dx, y + dy];
                            if (supportPiece != null && 
                                supportPiece.type == ChessPieceType.Pawn && 
                                supportPiece.team == team)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        public static int GetDistanceToPromotion(int y, int team)
        {
            return team == GameConstants.WHITE_TEAM 
                ? GameConstants.WHITE_PROMOTION_RANK - y 
                : y - GameConstants.BLACK_PROMOTION_RANK;
        }
        
        public static int GetBackRank(int team)
        {
            return team == GameConstants.WHITE_TEAM 
                ? GameConstants.WHITE_BACK_RANK 
                : GameConstants.BLACK_BACK_RANK;
        }
        
        public static int GetTeamRelativeY(int y, int team)
        {
            return team == GameConstants.WHITE_TEAM ? y : GameConstants.MAX_BOARD_INDEX - y;
        }
        
        public static float GetCenterControlValue(int x, int y)
        {
            float centerX = 3.5f;
            float centerY = 3.5f;
            return AIConstants.CENTER_SQUARE_BONUS - (Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY));
        }
    }
}
