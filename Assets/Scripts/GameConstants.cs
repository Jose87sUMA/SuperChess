namespace ChessGame
{
    /// <summary>
    /// Global constants that can be used throughout the entire chess game codebase
    /// </summary>
    public static class GameConstants
    {
        // Board dimensions
        public const int BOARD_SIZE = 8;
        public const int MAX_BOARD_INDEX = 7;
        public const int MIN_BOARD_INDEX = 0;
        
        // Teams
        public const int WHITE_TEAM = 0;
        public const int BLACK_TEAM = 1;
        public const int TEAM_COUNT = 2;
        
        // Ranks for different teams
        public const int WHITE_BACK_RANK = 0;
        public const int BLACK_BACK_RANK = 7;
        public const int WHITE_PROMOTION_RANK = 7;
        public const int BLACK_PROMOTION_RANK = 0;
        public const int WHITE_PAWN_START_RANK = 1;
        public const int BLACK_PAWN_START_RANK = 6;
        
        // Piece values (standard chess values)
        public const float PAWN_VALUE = 1f;
        public const float KNIGHT_VALUE = 3.2f;
        public const float BISHOP_VALUE = 3.3f;
        public const float ROOK_VALUE = 5f;
        public const float QUEEN_VALUE = 9f;
        public const float KING_VALUE = 900f;
        
        // Board center coordinates
        public const float BOARD_CENTER_X = 3.5f;
        public const float BOARD_CENTER_Y = 3.5f;
        
        // Common directions
        public const int FORWARD_WHITE = 1;
        public const int FORWARD_BLACK = -1;
        
        // File and rank constants
        public const int A_FILE = 0;
        public const int B_FILE = 1;
        public const int C_FILE = 2;
        public const int D_FILE = 3;
        public const int E_FILE = 4;
        public const int F_FILE = 5;
        public const int G_FILE = 6;
        public const int H_FILE = 7;
        
        public const int RANK_1 = 0;
        public const int RANK_2 = 1;
        public const int RANK_3 = 2;
        public const int RANK_4 = 3;
        public const int RANK_5 = 4;
        public const int RANK_6 = 5;
        public const int RANK_7 = 6;
        public const int RANK_8 = 7;
        
        // Knight move offsets
        public static readonly int[] KNIGHT_MOVE_X = { -2, -2, -1, -1, 1, 1, 2, 2 };
        public static readonly int[] KNIGHT_MOVE_Y = { -1, 1, -2, 2, -2, 2, -1, 1 };
        
        // King move offsets
        public static readonly int[] KING_MOVE_X = { -1, -1, -1, 0, 0, 1, 1, 1 };
        public static readonly int[] KING_MOVE_Y = { -1, 0, 1, -1, 1, -1, 0, 1 };
        
        // Center squares
        public static readonly UnityEngine.Vector2Int[] CENTER_SQUARES = {
            new(3, 3), new(3, 4), new(4, 3), new(4, 4)
        };
        
        // Extended center squares
        public static readonly UnityEngine.Vector2Int[] EXTENDED_CENTER_SQUARES = {
            new(2, 2), new(2, 3), new(2, 4), new(2, 5),
            new(3, 2), new(3, 3), new(3, 4), new(3, 5),
            new(4, 2), new(4, 3), new(4, 4), new(4, 5),
            new(5, 2), new(5, 3), new(5, 4), new(5, 5)
        };
        
        // Game states
        public const int GAME_ONGOING = 0;
        public const int GAME_WHITE_WINS = 1;
        public const int GAME_BLACK_WINS = 2;
        public const int GAME_DRAW = 3;
        
        // Network settings
        public const ushort DEFAULT_PORT = 8007;
        public const int MAX_CONNECTIONS = 2;
        
        // Card system
        public const int DEFAULT_HAND_SIZE = 5;
        public const int DEFAULT_DECK_SIZE = 20;
        public const int DEFAULT_DECK_COST_LIMIT = 60;
    }
    
    /// <summary>
    /// Utility methods for game constants
    /// </summary>
    public static class GameUtils
    {
        /// <summary>
        /// Gets the opposite team
        /// </summary>
        public static int GetOppositeTeam(int team)
        {
            return 1 - team;
        }
        
        /// <summary>
        /// Checks if coordinates are within board bounds
        /// </summary>
        public static bool IsValidPosition(int x, int y)
        {
            return x >= GameConstants.MIN_BOARD_INDEX && x <= GameConstants.MAX_BOARD_INDEX && 
                   y >= GameConstants.MIN_BOARD_INDEX && y <= GameConstants.MAX_BOARD_INDEX;
        }
        
        /// <summary>
        /// Gets the forward direction for a team
        /// </summary>
        public static int GetForwardDirection(int team)
        {
            return team == GameConstants.WHITE_TEAM ? GameConstants.FORWARD_WHITE : GameConstants.FORWARD_BLACK;
        }
        
        /// <summary>
        /// Gets the back rank for a team
        /// </summary>
        public static int GetBackRank(int team)
        {
            return team == GameConstants.WHITE_TEAM ? GameConstants.WHITE_BACK_RANK : GameConstants.BLACK_BACK_RANK;
        }
        
        /// <summary>
        /// Gets the promotion rank for a team
        /// </summary>
        public static int GetPromotionRank(int team)
        {
            return team == GameConstants.WHITE_TEAM ? GameConstants.WHITE_PROMOTION_RANK : GameConstants.BLACK_PROMOTION_RANK;
        }
        
        /// <summary>
        /// Gets the starting rank for pawns of a team
        /// </summary>
        public static int GetPawnStartRank(int team)
        {
            return team == GameConstants.WHITE_TEAM ? GameConstants.WHITE_PAWN_START_RANK : GameConstants.BLACK_PAWN_START_RANK;
        }
        
        /// <summary>
        /// Converts coordinates for team perspective (flips for black)
        /// </summary>
        public static int GetTeamRelativeY(int y, int team)
        {
            return team == GameConstants.WHITE_TEAM ? y : GameConstants.MAX_BOARD_INDEX - y;
        }
    }
}
