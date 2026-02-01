using ChessGame;

namespace AI
{
  /// <summary>
  /// Constants specific to AI evaluation and search algorithms
  /// </summary>
  public static class AIConstants
  {
    // Evaluation weights
    public const float MATERIAL_WEIGHT = 1.0f;
    public const float POSITIONAL_WEIGHT = 0.6f;
    public const float KING_SAFETY_WEIGHT = 1.0f;
    public const float MOBILITY_WEIGHT = 0.4f;
    public const float PAWN_STRUCTURE_WEIGHT = 0.5f;
    public const float CENTER_CONTROL_WEIGHT = 0.3f;
    public const float COORDINATION_WEIGHT = 0.4f;
    public const float ENDGAME_WEIGHT = 0.8f;
    // Search parameters
    public const int QUIESCENCE_DEPTH = 6;
    public const int MAX_OPENING_MOVES = 6;

    // Move scoring values
    public const float PREVIOUS_BEST_BONUS = 10000f;
    public const float CAPTURE_MULTIPLIER = 10f;
    public const float PROMOTION_BONUS = 8f;
    public const float CENTER_MULTIPLIER = 0.1f;
    public const float DEVELOPMENT_BONUS = 1.5f;
    public const float CHECK_BONUS = 3f;

    // Positional evaluation values
    public const float CENTER_SQUARE_BONUS = 4f;
    public const float DOUBLED_PAWN_PENALTY = 0.5f;
    public const float ISOLATED_PAWN_PENALTY = 0.5f;
    public const float MOBILITY_MULTIPLIER = 0.1f;
    public const float PASSED_PAWN_BASE_VALUE = 1.0f;

    // Card evaluation thresholds
    public const float CARD_PREFERENCE_THRESHOLD = 1f;
    public const float MIN_PAWN_PUSHES_FOR_QUICKSTEP = 2f;
    public const float QUICKSTEP_PENALTY = -5f;
    public const float QUICKSTEP_PAWN_BONUS = 0.25f;
    public const float PASSED_PAWN_CREATION_BONUS = 1.0f;
    // Endgame thresholds
    public const float ENDGAME_MATERIAL_THRESHOLD = 20f;
    public const float KING_ACTIVITY_MULTIPLIER = 0.2f;
    public const float KING_DISTANCE_MULTIPLIER = 0.2f;
    public const float PASSED_PAWN_ENDGAME_MULTIPLIER = 2f;        // Time controls
    public const float MIN_THINK_TIME = 0.4f;
    public const float MAX_THINK_TIME = 0.8f;
        
    public static float GetPawnPositionalValue(int x, int y)
    {
        float[,] table = {
            {9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f},
            {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
            {0.1f, 0.1f, 0.2f, 0.3f, 0.3f, 0.2f, 0.1f, 0.1f},
            {0.05f, 0.05f, 0.1f, 0.25f, 0.25f, 0.1f, 0.05f, 0.05f},
            {0.0f, 0.0f, 0.0f, 0.2f, 0.2f, 0.0f, 0.0f, 0.0f},
            {0.05f, -0.05f, -0.1f, 0.0f, 0.0f, -0.1f, -0.05f, 0.05f},
            {0.05f, 0.1f, 0.1f, -0.2f, -0.2f, 0.1f, 0.1f, 0.05f},
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f}
        };
        return table[y, x];
    }
    public static float GetKnightPositionalValue(int x, int y)
    {
        float[,] table = {
            {-0.5f, -0.4f, -0.3f, -0.3f, -0.3f, -0.3f, -0.4f, -0.5f},
            {-0.4f, -0.2f, 0.0f, 0.0f, 0.0f, 0.0f, -0.2f, -0.4f},
            {-0.3f, 0.0f, 0.1f, 0.15f, 0.15f, 0.1f, 0.0f, -0.3f},
            {-0.3f, 0.05f, 0.15f, 0.2f, 0.2f, 0.15f, 0.05f, -0.3f},
            {-0.3f, 0.0f, 0.15f, 0.2f, 0.2f, 0.15f, 0.0f, -0.3f},
            {-0.3f, 0.05f, 0.1f, 0.15f, 0.15f, 0.1f, 0.05f, -0.3f},
            {-0.4f, -0.2f, 0.0f, 0.05f, 0.05f, 0.0f, -0.2f, -0.4f},
            {-0.5f, -0.4f, -0.3f, -0.3f, -0.3f, -0.3f, -0.4f, -0.5f}
        };
        return table[y, x];
    }
    
    public static float GetBishopPositionalValue(int x, int y)
    {
        float[,] table = {
            {-0.2f, -0.1f, -0.1f, -0.1f, -0.1f, -0.1f, -0.1f, -0.2f},
            {-0.1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.1f},
            {-0.1f, 0.0f, 0.05f, 0.1f, 0.1f, 0.05f, 0.0f, -0.1f},
            {-0.1f, 0.05f, 0.05f, 0.1f, 0.1f, 0.05f, 0.05f, -0.1f},
            {-0.1f, 0.0f, 0.1f, 0.1f, 0.1f, 0.1f, 0.0f, -0.1f},
            {-0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, -0.1f},
            {-0.1f, 0.05f, 0.0f, 0.0f, 0.0f, 0.0f, 0.05f, -0.1f},
            {-0.2f, -0.1f, -0.1f, -0.1f, -0.1f, -0.1f, -0.1f, -0.2f}
        };
        return table[y, x];
    }
    
    public static float GetRookPositionalValue(int x, int y)
    {
        float[,] table = {
            {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            {0.05f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.05f},
            {-0.05f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.05f},
            {-0.05f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.05f},
            {-0.05f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.05f},
            {-0.05f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.05f},
            {-0.05f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.05f},
            {0.0f, 0.0f, 0.0f, 0.05f, 0.05f, 0.0f, 0.0f, 0.0f}
        };
        return table[y, x];
    }
    
    public static float GetQueenPositionalValue(int x, int y)
    {
        float[,] table = {
            {-0.2f, -0.1f, -0.1f, -0.05f, -0.05f, -0.1f, -0.1f, -0.2f},
            {-0.1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.1f},
            {-0.1f, 0.0f, 0.05f, 0.05f, 0.05f, 0.05f, 0.0f, -0.1f},
            {-0.05f, 0.0f, 0.05f, 0.05f, 0.05f, 0.05f, 0.0f, -0.05f},
            {0.0f, 0.0f, 0.05f, 0.05f, 0.05f, 0.05f, 0.0f, -0.05f},
            {-0.1f, 0.05f, 0.05f, 0.05f, 0.05f, 0.05f, 0.0f, -0.1f},
            {-0.1f, 0.0f, 0.05f, 0.0f, 0.0f, 0.0f, 0.0f, -0.1f},
            {-0.2f, -0.1f, -0.1f, -0.05f, -0.05f, -0.1f, -0.1f, -0.2f}
        };
        return table[y, x];
    }
    
    public static float GetKingPositionalValue(int x, int y)
    {
        float[,] table = {
            {-0.3f, -0.4f, -0.4f, -0.5f, -0.5f, -0.4f, -0.4f, -0.3f},
            {-0.3f, -0.4f, -0.4f, -0.5f, -0.5f, -0.4f, -0.4f, -0.3f},
            {-0.3f, -0.4f, -0.4f, -0.5f, -0.5f, -0.4f, -0.4f, -0.3f},
            {-0.3f, -0.4f, -0.4f, -0.5f, -0.5f, -0.4f, -0.4f, -0.3f},
            {-0.2f, -0.3f, -0.3f, -0.4f, -0.4f, -0.3f, -0.3f, -0.2f},
            {-0.1f, -0.2f, -0.2f, -0.2f, -0.2f, -0.2f, -0.2f, -0.1f},
            {0.2f, 0.2f, 0.0f, 0.0f, 0.0f, 0.0f, 0.2f, 0.2f},                
            {0.2f, 0.3f, 0.1f, 0.0f, 0.0f, 0.1f, 0.3f, 0.2f}
        };
        return table[y, x];
    }
  }
}
