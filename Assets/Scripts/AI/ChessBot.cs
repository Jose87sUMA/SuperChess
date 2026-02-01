using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardLogic;
using Cards.Runtime;
using ChessGame;
using ChessPieces;
using UnityEngine;
using Localization;
using Random = UnityEngine.Random;

namespace AI
{
    public sealed class ChessBot : MonoBehaviour
    {
        private struct Move { public Vector2Int From, To; }

        
        [SerializeField] private Chessboard  board;
        [SerializeField] private CardSystem  cardSystem;
        [SerializeField, Tooltip("0 = White, 1 = Black")] private int myTeam = 1;
        [SerializeField] private bool active;
        [SerializeField, Range(1, 3)] private int searchDepth = 2;
        public bool IsActive => active;
        
        public int MyTeam => myTeam;
        private int _moveCount;
        private CancellationTokenSource _cancellationTokenSource;        private bool _playedMagicStorm = false;
        private bool _playedStrategicJump = false;
        private Move? _strategicJumpMove = null;
        
        private bool _wasThinking = false;
        private bool _originalDragEnabled = true;
        private bool _originalCardPlayingState = false;
        
        private class BoardState
        {
            public ChessPiece[,] Pieces;
            public bool[] CanJump;
            public int CurrentTurn;
        }
        
        private Dictionary<string, List<(Vector2Int from, Vector2Int to)>> _openingBook = new Dictionary<string, List<(Vector2Int, Vector2Int)>>
        {
            ["start"] = new List<(Vector2Int, Vector2Int)>
            {
                (new Vector2Int(4, 1), new Vector2Int(4, 3)), // e2-e4
                (new Vector2Int(3, 1), new Vector2Int(3, 3)), // d2-d4
                (new Vector2Int(6, 0), new Vector2Int(5, 2)), // Nf3
                (new Vector2Int(1, 0), new Vector2Int(2, 2)), // Nc3
            },
            ["e4"] = new List<(Vector2Int, Vector2Int)>
            {
                (new Vector2Int(6, 0), new Vector2Int(5, 2)), // Nf3
                (new Vector2Int(5, 0), new Vector2Int(2, 3)), // Bc4
                (new Vector2Int(1, 0), new Vector2Int(2, 2)), // Nc3
            },
            ["d4"] = new List<(Vector2Int, Vector2Int)>
            {
                (new Vector2Int(6, 0), new Vector2Int(5, 2)), // Nf3
                (new Vector2Int(2, 1), new Vector2Int(2, 3)), // c2-c4
                (new Vector2Int(1, 0), new Vector2Int(2, 2)), // Nc3
            }
        };
        
        private void Awake()
        {
            board.TurnStarted  += OnTurnStarted;
            board.switchCameraOnPlay = !active;
        }

        public void SetActive(bool value)
        {
            active = value;
            board.switchCameraOnPlay = !value;
            if (value) _moveCount = 0;
        }

        public void ConfigureAI(int depth)
        {
            searchDepth = Mathf.Clamp(depth, 1, 3);
            SetActive(true);
        }
        
        private void OnDestroy()
        {
            board.TurnStarted  -= OnTurnStarted;
            CancelSearch();
        }
        
        
        private void OnTurnStarted(int team)
        {
            if (team != myTeam || !active) return;

            CancelSearch();
            _playedMagicStorm = false;
            _playedStrategicJump = false;
            StartCoroutine(PlayTurn());
        }

        private void CancelSearch()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (_wasThinking && board != null && cardSystem != null)
            {
                board.dragEnabled = _originalDragEnabled;
                cardSystem.currentlyPlayingCard = _originalCardPlayingState;
                _wasThinking = false;
            }
        }

        
        private IEnumerator PlayTurn()
        {
            while (board.currentTurn == myTeam)
            {
                yield return new WaitForSeconds(Random.Range(0.4f, 0.8f));

                board.EvaluateBoardState(myTeam, out bool kingInCheck, out bool hasLegalMove);
                if (!hasLegalMove) break;

                if (_moveCount < AIConstants.MAX_OPENING_MOVES && !kingInCheck)
                {
                    var openingMove = GetOpeningBookMove();
                    if (openingMove.HasValue)
                    {
                        board.MoveTo(openingMove.Value.from.x, openingMove.Value.from.y,
                                   openingMove.Value.to.x, openingMove.Value.to.y);
                        _moveCount++;
                        continue;
                    }
                }                var handUI = cardSystem.GetComponent<HandUI>();
                handUI?.ShowInfo(LocalizationManager.Instance.Get("ui.ai.thinking"));

                _originalDragEnabled = board.dragEnabled;
                _originalCardPlayingState = cardSystem.currentlyPlayingCard;
                _wasThinking = true;
                board.dragEnabled = false;
                cardSystem.currentlyPlayingCard = true;

                _cancellationTokenSource = new CancellationTokenSource();

                Task<(Move?, float)> searchTask = null;
                yield return StartCoroutine(RunAsyncSearch((task) => searchTask = task));

                yield return new WaitUntil(() => searchTask.IsCompleted || searchTask.IsCanceled || searchTask.IsFaulted);
                
                if (_wasThinking)
                {
                    board.dragEnabled = _originalDragEnabled;
                    cardSystem.currentlyPlayingCard = _originalCardPlayingState;
                    _wasThinking = false;
                }
                
                if (searchTask.IsCanceled || searchTask.IsFaulted)
                {
                    handUI?.ShowInfo(LocalizationManager.Instance.Get("ui.ai.searchInterrupted"));
                    break;
                }

                Move? bestMove = searchTask.Result.Item1;
                float bestMoveScore = searchTask.Result.Item2;

                CardInstance bestCard = null;
                float bestCardScore = float.NegativeInfinity;
                if (!kingInCheck)
                    bestCard = EvaluateHand(out bestCardScore);

                if (bestCard != null && bestCardScore > bestMoveScore + AIConstants.CARD_PREFERENCE_THRESHOLD)
                {
                    PlayCard(bestCard);
                }
                else if (bestMove != null)
                {
                    ChessPiece piece = board.ChessPieces[bestMove.Value.From.x, bestMove.Value.From.y];
                    List<Vector2Int> allMoves = piece.GetAvailableMoves(board, 8, 8);
                    board.specialMove = piece.GetSpecialMoves(ref board.ChessPieces, ref board.MoveList, ref allMoves);
                    board.MoveTo(bestMove.Value.From.x, bestMove.Value.From.y, bestMove.Value.To.x, bestMove.Value.To.y);
                    _moveCount++;
                }
                else
                {
                    break;
                }
            }
        }

        private (Vector2Int from, Vector2Int to)? GetOpeningBookMove()
        {
            List<(Vector2Int, Vector2Int)> candidates = null;
            
            if (_moveCount == 0)
            {
                candidates = _openingBook["start"];
            }
            else if (_moveCount <= 3)
            {
                candidates = new List<(Vector2Int, Vector2Int)>
                {
                    (new Vector2Int(6, 0), new Vector2Int(5, 2)), // Nf3
                    (new Vector2Int(1, 0), new Vector2Int(2, 2)), // Nc3
                    (new Vector2Int(5, 0), new Vector2Int(2, 3)), // Bc4
                    (new Vector2Int(2, 1), new Vector2Int(2, 3)), // c2-c4
                };
            }
            
            if (candidates == null) return null;
            
          var legalCandidates = new List<(Vector2Int, Vector2Int)>();
            foreach (var (from, to) in candidates)
            {
                var actualFrom = myTeam == 0 ? from : new Vector2Int(from.x, 7 - from.y);
                var actualTo = myTeam == 0 ? to : new Vector2Int(to.x, 7 - to.y);
                
                var piece = board.ChessPieces[actualFrom.x, actualFrom.y];
                if (piece && piece.team == myTeam)
                {
                    var legalMoves = board.FilteredMoves(piece);
                    if (legalMoves.Contains(actualTo) && 
                        !board.MoveLeavesKingInCheck(actualFrom, actualTo, myTeam))
                    {
                        legalCandidates.Add((actualFrom, actualTo));
                    }
                }
            }
            
            if (legalCandidates.Count == 0) return null;
            
            var selected = legalCandidates[Random.Range(0, legalCandidates.Count)];
            return selected;
        }
        
        private IEnumerator RunAsyncSearch(System.Action<Task<(Move?, float)>> callback)
        {
            var task = SearchAsync(_cancellationTokenSource.Token);
            callback(task);
            yield return null;
        }
        
        
        
        private async Task<(Move?, float)> SearchAsync(CancellationToken cancellationToken)
        {
            try
            {
                BoardState boardState = await Task.Run(() => CaptureThreadSafeBoardState(), cancellationToken);
                
                var result = await Task.Run(() => MinimaxRootThreaded(boardState, cancellationToken), cancellationToken);
                
                return result;
            }
            catch (System.OperationCanceledException)
            {
                return (null, float.NegativeInfinity);
            }
            catch (System.Exception)
            {
                return (null, float.NegativeInfinity);
            }
        }

        private BoardState CaptureThreadSafeBoardState()
        {
            var pieces = new ChessPiece[GameConstants.BOARD_SIZE, GameConstants.BOARD_SIZE];
            var jumpStates = new bool[2];

            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                {
                    pieces[x, y] = board.ChessPieces[x, y];
                }
            }

            jumpStates[GameConstants.WHITE_TEAM] = board.canJump[GameConstants.WHITE_TEAM];
            jumpStates[GameConstants.BLACK_TEAM] = board.canJump[GameConstants.BLACK_TEAM];

            return new BoardState
            {
                Pieces = pieces,
                CanJump = jumpStates,
                CurrentTurn = board.currentTurn
            };
        }

        private (Move?, float) MinimaxRootThreaded(BoardState boardState, CancellationToken cancellationToken)
        {
            Move? best = null;
            float bestScore = float.NegativeInfinity;

            for (int depth = 1; depth <= searchDepth; depth++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                float alpha = float.NegativeInfinity;
                float beta = float.PositiveInfinity;
                Move? currentBest = null;
    
                var moves = GenerateLegalMovesThreaded(boardState, myTeam);

                moves.Sort((a, b) => ScoreMove(boardState, a, best).CompareTo(ScoreMove(boardState, b, best)));
                moves.Reverse();

                foreach (Move mv in moves)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var newState = MakeTemporaryMoveThreaded(boardState, mv);
                    float score = SearchThreaded(newState, depth - 1, alpha, beta, 1 - myTeam, false, cancellationToken);

                    if (score > alpha)
                    {
                        alpha = score;
                        currentBest = mv;
                    }
                }

                if (currentBest.HasValue)
                {
                    best = currentBest;
                    bestScore = alpha;
                }

            }

            return (best, bestScore);
        }

        private float ScoreMove(BoardState boardState, Move move, Move? previousBest)
        {
            float score = 0f;

            if (previousBest.HasValue && move.From == previousBest.Value.From && move.To == previousBest.Value.To)
                score += AIConstants.PREVIOUS_BEST_BONUS;

            var target = boardState.Pieces[move.To.x, move.To.y];
            var movingPiece = boardState.Pieces[move.From.x, move.From.y];

            if (target)
            {
                if (target.type == ChessPieceType.King)
                {
                    return 50000f;
                }

                score += PieceValue(target.type) * AIConstants.CAPTURE_MULTIPLIER;
                score += (PieceValue(target.type) - PieceValue(movingPiece.type));
            }

            if (movingPiece.type == ChessPieceType.Pawn)
            {
                int promotionRank = movingPiece.team == GameConstants.WHITE_TEAM ? GameConstants.WHITE_PROMOTION_RANK : GameConstants.BLACK_PROMOTION_RANK;
                if (move.To.y == promotionRank)
                    score += AIConstants.PROMOTION_BONUS;
            }

            float centerValue = EvaluationHelper.GetCenterControlValue(move.To.x, move.To.y);
            score += centerValue * AIConstants.CENTER_MULTIPLIER;

            if (movingPiece.type != ChessPieceType.Pawn && movingPiece.type != ChessPieceType.King)
            {
                int backRank = EvaluationHelper.GetBackRank(movingPiece.team);
                if (move.From.y == backRank)
                    score += AIConstants.DEVELOPMENT_BONUS;
            }

            var tempState = MakeTemporaryMoveThreaded(boardState, move);
            Vector2Int enemyKing = FindKing(tempState, 1 - movingPiece.team);
            if (enemyKing.x >= 0 && IsInCheck(tempState, enemyKing, 1 - movingPiece.team))
                score += AIConstants.CHECK_BONUS;

            if (IsSquareAttacked(tempState, move.To, 1 - movingPiece.team))
                score -= PieceValue(movingPiece.type) * 0.5f;

            if (movingPiece.type == ChessPieceType.King && Mathf.Abs(move.To.x - move.From.x) == 2)
                score += 2f;

            return score;
        }
        private float SearchThreaded(BoardState boardState, int depth, float alpha, float beta, int side, bool maximizing, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (depth == 0)
            {
                return QuiescenceSearch(boardState, alpha, beta, side, maximizing, AIConstants.QUIESCENCE_DEPTH, cancellationToken);
            }

            var moves = GenerateLegalMovesThreaded(boardState, side);

            if (moves.Count == 0)
            {
                Vector2Int king = FindKing(boardState, side);
                if (king.x >= 0 && IsInCheck(boardState, king, side))
                {
                    return maximizing ? (-10000f + depth) : (10000f - depth);
                }
                else
                {
                    return 0f;
                }
            }

            moves.Sort((a, b) => ScoreMove(boardState, a, null).CompareTo(ScoreMove(boardState, b, null)));
            if (maximizing) moves.Reverse();

            if (maximizing)
            {
                float maxEval = float.NegativeInfinity;
                foreach (Move mv in moves)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var newState = MakeTemporaryMoveThreaded(boardState, mv);
                    float eval = SearchThreaded(newState, depth - 1, alpha, beta, 1 - side, false, cancellationToken);

                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);

                    if (beta <= alpha) break;
                }
                return maxEval;
            }
            else
            {
                float minEval = float.PositiveInfinity;
                foreach (Move mv in moves)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var newState = MakeTemporaryMoveThreaded(boardState, mv);
                    float eval = SearchThreaded(newState, depth - 1, alpha, beta, 1 - side, true, cancellationToken);

                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);

                    if (beta <= alpha) break;
                }
                return minEval;
            }
        }
        
        private float QuiescenceSearch(BoardState boardState, float alpha, float beta, int side, bool maximizing, int depth, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            float standPat = EvaluateBoardThreaded(boardState);
            
            if (depth <= 0)
                return standPat;
            
            if (maximizing)
            {
                if (standPat >= beta)
                    return beta;
                alpha = Mathf.Max(alpha, standPat);
            }
            else
            {
                if (standPat <= alpha)
                    return alpha;
                beta = Mathf.Min(beta, standPat);
            }
            
            var captures = GenerateCapturesThreaded(boardState, side);
            
            foreach (Move capture in captures)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var newState = MakeTemporaryMoveThreaded(boardState, capture);
                float score = QuiescenceSearch(newState, alpha, beta, 1 - side, !maximizing, depth - 1, cancellationToken);
                
                if (maximizing)
                {
                    alpha = Mathf.Max(alpha, score);
                    if (alpha >= beta) break;
                }
                else
                {
                    beta = Mathf.Min(beta, score);
                    if (beta <= alpha) break;
                }
            }
            
            return maximizing ? alpha : beta;
        }

        private List<Move> GenerateCapturesThreaded(BoardState boardState, int team)
        {
            var captures = new List<Move>();

            BoardHelper.ForEachPieceOfTeam(boardState.Pieces, team, (piece, x, y) =>
            {
                Vector2Int from = new(x, y);
                var moves = GetPieceMovesThreaded(boardState, piece);

                foreach (Vector2Int dst in moves)
                {
                    var target = boardState.Pieces[dst.x, dst.y];
                    if (target && target.team != team)
                    {
                        if (!MoveLeavesKingInCheckThreaded(boardState, from, dst, team))
                            captures.Add(new Move { From = from, To = dst });
                    }
                }
            });

            captures.Sort((a, b) =>
            {
                var victimA = boardState.Pieces[a.To.x, a.To.y];
                var victimB = boardState.Pieces[b.To.x, b.To.y];
                var attackerA = boardState.Pieces[a.From.x, a.From.y];
                var attackerB = boardState.Pieces[b.From.x, b.From.y];

                float scoreA = PieceValue(victimA.type) - PieceValue(attackerA.type);
                float scoreB = PieceValue(victimB.type) - PieceValue(attackerB.type);

                return scoreB.CompareTo(scoreA);
            });

            return captures;
        }

        private bool IsInCheck(BoardState boardState, Vector2Int kingPos, int team)
        {
            bool inCheck = false;
            BoardHelper.ForEachPieceOfTeam(boardState.Pieces, 1 - team, (piece, x, y) =>
            {
                var attacks = GetPieceMovesThreaded(boardState, piece);
                if (attacks.Contains(kingPos))
                    inCheck = true;
            });
            return inCheck;
        }
        
        private bool IsSquareAttacked(BoardState boardState, Vector2Int square, int byTeam)
        {
            bool isAttacked = false;
            BoardHelper.ForEachPieceOfTeam(boardState.Pieces, byTeam, (piece, x, y) =>
            {
                var attacks = GetPieceMovesThreaded(boardState, piece);
                if (attacks.Contains(square))
                    isAttacked = true;
            });
            return isAttacked;
        }

        private List<Move> GenerateLegalMovesThreaded(BoardState boardState, int team)
        {
            var moves = new List<Move>();

            BoardHelper.ForEachPieceOfTeam(boardState.Pieces, team, (piece, x, y) =>
            {
                Vector2Int from = new(x, y);
                var pieceMoves = GetPieceMovesThreaded(boardState, piece);

                foreach (Vector2Int dst in pieceMoves)
                {
                    if (MoveLeavesKingInCheckThreaded(boardState, from, dst, team)) continue;
                    moves.Add(new Move { From = from, To = dst });
                }
            });

            return moves;
        }
        
        private List<Vector2Int> GetPieceMovesThreaded(BoardState boardState, ChessPiece piece)
        {
            var moves = new List<Vector2Int>();
            int x = piece.currentX;
            int y = piece.currentY;
            
            switch (piece.type)
            {                case ChessPieceType.Pawn:
                    int dir = piece.team == GameConstants.WHITE_TEAM ? 1 : -1;
                    
                    if (GameUtils.IsValidPosition(x, y + dir) && !boardState.Pieces[x, y + dir])
                    {
                        moves.Add(new Vector2Int(x, y + dir));
                        bool isStartingRank = (piece.team == GameConstants.WHITE_TEAM && y == 1) || 
                                            (piece.team == GameConstants.BLACK_TEAM && y == 6);
                        if (isStartingRank && (!boardState.Pieces[x, y + 2 * dir] || boardState.CanJump[piece.team]))
                            moves.Add(new Vector2Int(x, y + 2 * dir));
                    }
                    
                    for (int dx = -1; dx <= 1; dx += 2)
                    {
                        if (GameUtils.IsValidPosition(x + dx, y + dir))
                        {
                            var target = boardState.Pieces[x + dx, y + dir];
                            if (target && target.team != piece.team)
                                moves.Add(new Vector2Int(x + dx, y + dir));
                        }
                    }
                    break;
                    
                case ChessPieceType.Rook:
                    AddLinearMovesThreaded(boardState, moves, x, y, piece.team, true, false);
                    break;
                    
                case ChessPieceType.Bishop:
                    AddLinearMovesThreaded(boardState, moves, x, y, piece.team, false, true);
                    break;
                    
                case ChessPieceType.Queen:
                    AddLinearMovesThreaded(boardState, moves, x, y, piece.team, true, true);
                    break;
                      case ChessPieceType.Knight:
                    int[] knightX = { -2, -2, -1, -1, 1, 1, 2, 2 };
                    int[] knightY = { -1, 1, -2, 2, -2, 2, -1, 1 };
                    for (int i = 0; i < 8; i++)
                    {
                        int nx = x + knightX[i];
                        int ny = y + knightY[i];
                        if (BoardHelper.IsValidPosition(nx, ny))
                        {
                            var target = boardState.Pieces[nx, ny];
                            if (!target || target.team != piece.team)
                                moves.Add(new Vector2Int(nx, ny));
                        }
                    }
                    break;
                    
                case ChessPieceType.King:
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = x + dx;
                            int ny = y + dy;
                            if (BoardHelper.IsValidPosition(nx, ny))
                            {
                                var target = boardState.Pieces[nx, ny];
                                if (!target || target.team != piece.team)
                                    moves.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }
                    break;
            }
            
            return moves;
        }

        private void AddLinearMovesThreaded(BoardState boardState, List<Vector2Int> moves, int x, int y, int team, bool orthogonal, bool diagonal)
        {
            int[] dx = orthogonal && diagonal ? new[] { -1, -1, -1, 0, 0, 1, 1, 1 } :
                      orthogonal ? new[] { -1, 0, 1, 0 } :
                      new[] { -1, -1, 1, 1 };

            int[] dy = orthogonal && diagonal ? new[] { -1, 0, 1, -1, 1, -1, 0, 1 } :
                      orthogonal ? new[] { 0, -1, 0, 1 } :
                      new[] { -1, 1, -1, 1 };
            for (int d = 0; d < dx.Length; d++)
            {
                bool jumpConsumed = false;
                for (int dist = 1; dist < GameConstants.BOARD_SIZE; dist++)
                {
                    int nx = x + dx[d] * dist;
                    int ny = y + dy[d] * dist;

                    if (!GameUtils.IsValidPosition(nx, ny)) break;

                    var piece = boardState.Pieces[nx, ny];
                    if (!piece)
                    {
                        moves.Add(new Vector2Int(nx, ny));
                    }
                    else
                    {
                        if (piece.team != team)
                            moves.Add(new Vector2Int(nx, ny));

                        if (boardState.CanJump[team] && !jumpConsumed)
                        {
                            jumpConsumed = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private BoardState MakeTemporaryMoveThreaded(BoardState boardState, Move move)
        {
            var newState = new BoardState
            {
                Pieces = new ChessPiece[GameConstants.BOARD_SIZE, GameConstants.BOARD_SIZE],
                CanJump = new bool[2],
                CurrentTurn = boardState.CurrentTurn
            };

            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                {
                    newState.Pieces[x, y] = boardState.Pieces[x, y];
                }
            }

            newState.CanJump[GameConstants.WHITE_TEAM] = boardState.CanJump[GameConstants.WHITE_TEAM];
            newState.CanJump[GameConstants.BLACK_TEAM] = boardState.CanJump[GameConstants.BLACK_TEAM];

            var movingPiece = newState.Pieces[move.From.x, move.From.y];
            newState.Pieces[move.To.x, move.To.y] = movingPiece;
            newState.Pieces[move.From.x, move.From.y] = null;

            return newState;
        }

        private bool MoveLeavesKingInCheckThreaded(BoardState boardState, Vector2Int from, Vector2Int to, int team)
        {
            var tempState = MakeTemporaryMoveThreaded(boardState, new Move { From = from, To = to });

            Vector2Int kingPos = BoardHelper.FindPiece(tempState.Pieces, piece =>
                piece.type == ChessPieceType.King && piece.team == team);

            if (kingPos.x < 0) return true;

            bool isInCheck = false;
            BoardHelper.ForEachPieceOfTeam(tempState.Pieces, 1 - team, (piece, x, y) =>
            {
                var attacks = GetPieceMovesThreaded(tempState, piece);
                if (attacks.Contains(kingPos))
                {
                    isInCheck = true;
                }
            });

            return isInCheck;
        }        private float EvaluateBoardThreaded(BoardState boardState)
        {
            Vector2Int myKing = FindKing(boardState, myTeam);
            Vector2Int enemyKing = FindKing(boardState, 1 - myTeam);
            
            if (myKing.x < 0)
                return -999999f;
            if (enemyKing.x < 0)
                return 999999f;

            float materialScore = EvaluateMaterial(boardState);

            float positionalScore = EvaluatePositions(boardState);

            float kingSafetyScore = EvaluateKingSafety(boardState);

            float mobilityScore = EvaluateMobility(boardState);

            float pawnScore = EvaluatePawnStructure(boardState);

            float centerScore = EvaluateCenterControl(boardState);

            float coordinationScore = EvaluatePieceCoordination(boardState);

            float endgameScore = EvaluateEndgame(boardState);

            float score = materialScore * AIConstants.MATERIAL_WEIGHT +
                   positionalScore * AIConstants.POSITIONAL_WEIGHT +
                   kingSafetyScore * AIConstants.KING_SAFETY_WEIGHT +
                   mobilityScore * AIConstants.MOBILITY_WEIGHT +
                   pawnScore * AIConstants.PAWN_STRUCTURE_WEIGHT +
                   centerScore * AIConstants.CENTER_CONTROL_WEIGHT +
                   coordinationScore * AIConstants.COORDINATION_WEIGHT +
                   endgameScore * AIConstants.ENDGAME_WEIGHT;

            return score;
        }
        
        
        private float EvaluateMaterial(BoardState boardState)
        {
            float score = 0f;
            BoardHelper.ForEachPiece(boardState.Pieces, (piece, x, y) =>
            {
                float val = PieceValue(piece.type);
                score += (piece.team == myTeam) ? val : -val;
            });
            return score;
        }
        
        
        private float EvaluatePositions(BoardState boardState)
        {
            float score = 0f;
            
            BoardHelper.ForEachPiece(boardState.Pieces, (piece, x, y) =>
            {
                float posValue = GetPositionalValue(piece.type, x, y, piece.team);
                score += (piece.team == myTeam) ? posValue : -posValue;
            });
            
            return score;
        }
        private float GetPositionalValue(ChessPieceType type, int x, int y, int team)
        {
            int evalY = EvaluationHelper.GetTeamRelativeY(y, team);
            
            return type switch
            {
                ChessPieceType.Pawn => AIConstants.GetPawnPositionalValue(x, evalY),
                ChessPieceType.Knight => AIConstants.GetKnightPositionalValue(x, evalY),
                ChessPieceType.Bishop => AIConstants.GetBishopPositionalValue(x, evalY),
                ChessPieceType.Rook => AIConstants.GetRookPositionalValue(x, evalY),
                ChessPieceType.Queen => AIConstants.GetQueenPositionalValue(x, evalY),
                ChessPieceType.King => AIConstants.GetKingPositionalValue(x, evalY),
                _ => 0f
            };
        }
        
        
        
          private float EvaluateKingSafety(BoardState boardState)
        {
            float score = 0f;
            
            Vector2Int myKing = FindKing(boardState, myTeam);
            Vector2Int enemyKing = FindKing(boardState, 1 - myTeam);
            
            if (myKing.x >= 0)
            {
                score += CalculateKingSafety(boardState, myKing, myTeam);
            }
            
            if (enemyKing.x >= 0)
            {
                score -= CalculateKingSafety(boardState, enemyKing, 1 - myTeam);
            }
            
            return score;
        }

        private Vector2Int FindKing(BoardState boardState, int team)
        {
            return BoardHelper.FindPiece(boardState.Pieces, piece =>
                piece.type == ChessPieceType.King && piece.team == team);
        }

        private float CalculateKingSafety(BoardState boardState, Vector2Int kingPos, int team)
        {
            float safety = 0f;

            int dir = team == GameConstants.WHITE_TEAM ? 1 : -1;
            for (int dx = -1; dx <= 1; dx++)
            {
                int x = kingPos.x + dx;
                int y = kingPos.y + dir;
                if (BoardHelper.IsValidPosition(x, y))
                {
                    var piece = boardState.Pieces[x, y];
                    if (piece && piece.type == ChessPieceType.Pawn && piece.team == team)
                        safety += 0.5f;
                }
            }

            float centerDistance = EvaluationHelper.GetCenterControlValue(kingPos.x, kingPos.y);
            if (centerDistance > 1.0f)
                safety -= 0.3f;

            return safety;
        }
        
        
        private float EvaluateMobility(BoardState boardState)
        {
            float score = 0f;
            
            BoardHelper.ForEachPiece(boardState.Pieces, (piece, x, y) =>
            {
                int moveCount = GetPieceMovesThreaded(boardState, piece).Count;
                float mobilityScore = moveCount * AIConstants.MOBILITY_MULTIPLIER;
                
                score += (piece.team == myTeam) ? mobilityScore : -mobilityScore;
            });
            
            return score;
        }
        
        private float EvaluatePawnStructure(BoardState boardState)
        {
            float score = 0f;
            
            for (int team = 0; team < 2; team++)
            {
                float teamScore = 0f;
                
                for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
                {
                    List<int> pawnsInFile = new List<int>();
                    for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                    {
                        var piece = boardState.Pieces[x, y];
                        if (piece && piece.type == ChessPieceType.Pawn && piece.team == team)
                            pawnsInFile.Add(y);
                    }
                    
                    if (pawnsInFile.Count > 1)
                    {
                        teamScore -= AIConstants.DOUBLED_PAWN_PENALTY * (pawnsInFile.Count - 1);
                    }
                    
                    foreach (int y in pawnsInFile)
                    {
                        bool hasSupport = EvaluationHelper.HasPawnSupport(boardState.Pieces, x, y, team);

                        if (!hasSupport)
                            teamScore -= AIConstants.ISOLATED_PAWN_PENALTY;
                        
                        bool isPassed = EvaluationHelper.IsPassedPawn(boardState.Pieces, x, y, team);
                        
                        if (isPassed)
                        {
                            int distanceToPromotion = EvaluationHelper.GetDistanceToPromotion(y, team);
                            teamScore += AIConstants.PASSED_PAWN_BASE_VALUE / (distanceToPromotion + 1);
                        }
                    }
                }
                
                score += (team == myTeam) ? teamScore : -teamScore;
            }
            
            return score;
        }
        
        private float EvaluateCenterControl(BoardState boardState)
        {
            float score = 0f;
            Vector2Int[] centerSquares = { new(3, 3), new(3, 4), new(4, 3), new(4, 4) };
            
            foreach (var square in centerSquares)
            {
                var piece = boardState.Pieces[square.x, square.y];
                if (piece)
                {
                    float value = piece.type == ChessPieceType.Pawn ? 0.3f : 0.2f;
                    score += (piece.team == myTeam) ? value : -value;
                }
                  
                bool myTeamControls = false;
                bool enemyControls = false;
                
                BoardHelper.ForEachPiece(boardState.Pieces, (controlPiece, x, y) =>
                {
                    var moves = GetPieceMovesThreaded(boardState, controlPiece);
                    if (moves.Contains(square))
                    {
                        
                        if (controlPiece.team == myTeam)
                            myTeamControls = true;
                        else
                            enemyControls = true;
                    }
                });
                
                if (myTeamControls && !enemyControls)
                    score += 0.1f;
                else if (enemyControls && !myTeamControls)
                    score -= 0.1f;
            }
            
            return score;
        }

        private float EvaluatePieceCoordination(BoardState boardState)
        {
            float score = 0f;

            BoardHelper.ForEachPiece(boardState.Pieces, (piece, x, y) =>
            {
                var moves = GetPieceMovesThreaded(boardState, piece);
                int defendedPieces = 0;

                foreach (var move in moves)
                {
                    var defendedPiece = boardState.Pieces[move.x, move.y];
                    if (defendedPiece && defendedPiece.team == piece.team)
                        defendedPieces++;
                }

                float coordinationValue = defendedPieces * 0.1f;
                score += (piece.team == myTeam) ? coordinationValue : -coordinationValue;
            });
            return score;
        }
        
        private float EvaluateEndgame(BoardState boardState)
        {
            float score = 0f;
              int totalMaterial = 0;
            BoardHelper.ForEachPiece(boardState.Pieces, (piece, x, y) =>
            {
                if (piece.type != ChessPieceType.King && piece.type != ChessPieceType.Pawn)
                    totalMaterial += (int)PieceValue(piece.type);
            });
            
            bool isEndgame = totalMaterial < 20f;
            
            
              if (isEndgame)
            {
                Vector2Int myKing = FindKing(boardState, myTeam);
                Vector2Int enemyKing = FindKing(boardState, 1 - myTeam);
                
                if (myKing.x >= 0)
                {
                    float centerDistance = Mathf.Abs(myKing.x - 3.5f) + Mathf.Abs(myKing.y - 3.5f);
                    score += (7f - centerDistance) * 0.2f;
                    
                    float materialAdvantage = EvaluateMaterial(boardState);
                    if (materialAdvantage > 3f && enemyKing.x >= 0)
                    {
                        float enemyCenterDistance = Mathf.Abs(enemyKing.x - 3.5f) + Mathf.Abs(enemyKing.y - 3.5f);
                        score += enemyCenterDistance * 0.3f;
                        
                        float kingDistance = Mathf.Abs(myKing.x - enemyKing.x) + Mathf.Abs(myKing.y - enemyKing.y);
                        score += (14f - kingDistance) * 0.2f;
                    }
                }
                
                score += EvaluatePassedPawnsEndgame(boardState) * 2f;
            }
            
            return score;
        }
        
        private float EvaluatePassedPawnsEndgame(BoardState boardState)
        {
            float score = 0f;

            BoardHelper.ForEachPieceOfType(boardState.Pieces, ChessPieceType.Pawn, (piece, x, y) =>
            {
                
                if (EvaluationHelper.IsPassedPawn(boardState.Pieces, x, y, piece.team))
                {
                    int distanceToPromotion = EvaluationHelper.GetDistanceToPromotion(y, piece.team);
                    float passedPawnValue = 2.0f / (distanceToPromotion + 1);
                    score += (piece.team == myTeam) ? passedPawnValue : -passedPawnValue;
                }
            });

            return score;
        }
  
        private static float PieceValue(ChessPieceType t) => t switch
        {
            ChessPieceType.Pawn => GameConstants.PAWN_VALUE,
            ChessPieceType.Knight => GameConstants.KNIGHT_VALUE,
            ChessPieceType.Bishop => GameConstants.BISHOP_VALUE,
            ChessPieceType.Rook => GameConstants.ROOK_VALUE,
            ChessPieceType.Queen => GameConstants.QUEEN_VALUE,
            ChessPieceType.King => GameConstants.KING_VALUE,
            _ => 0f
        };
  

        private CardInstance EvaluateHand(out float bestScore)
        {
            CardInstance best = null;
            bestScore = float.NegativeInfinity;

            foreach (CardInstance inst in cardSystem.Hands[myTeam].Cards)
            {
                float sc = inst.Data.effectClassName switch
                {
                    "Cards.Effect.Effects.QuickStepEffect" => ScoreQuickStep(),
                    "Cards.Effect.Effects.StrategicJumpEffect" => ScoreStrategicJump(),
                    "Cards.Effect.Effects.MagicStormEffect" => ScoreMagicStorm(),
                    _ => -200f
                };

                if (sc > bestScore)
                {
                    bestScore = sc;
                    best = inst;
                }
            }
            return best;
        }

        private float ScoreQuickStep()
        {
            int pushes = CountActualPawnPushes();
            if (pushes < AIConstants.MIN_PAWN_PUSHES_FOR_QUICKSTEP) return -900f;

            float score = AIConstants.QUICKSTEP_PAWN_BONUS * pushes;

            foreach (var pawn in GetMyPawns())
            {
                if (CanCreatePassedPawn(pawn))
                    score += AIConstants.PASSED_PAWN_CREATION_BONUS;
            }

            return score;
        }
        
        private List<Pawn> GetMyPawns()
        {
            var pawns = new List<Pawn>();
            foreach (var p in board.ChessPieces)
            {
                if (p is Pawn pawn && p.team == myTeam)
                    pawns.Add(pawn);
            }
            return pawns;
        }

        private bool CanCreatePassedPawn(Pawn pawn)
        {
            return EvaluationHelper.IsPassedPawn(board.ChessPieces, pawn.currentX, pawn.currentY, pawn.team);
        }

        
        
        private int CountActualPawnPushes()
        {
            int cnt = 0;
            int dir = -1;

            foreach (var p in board.ChessPieces)
            {
                if (p is not Pawn pawn || p.team != GameConstants.BLACK_TEAM) continue;
                int nx = pawn.currentX;
                int ny = pawn.currentY + dir;
                if (BoardHelper.IsValidPosition(nx, ny) && !board.ChessPieces[nx, ny] && !board.MoveLeavesKingInCheck(new(pawn.currentX, pawn.currentY), new(nx, ny), myTeam))
                    ++cnt;
            }
            return cnt;
        }

        
        
        private float ScoreStrategicJump()
        {
            if (_playedStrategicJump)
                return -900;

            bool prev = board.canJump[myTeam];
            board.canJump[myTeam] = true;

            float gain = 0f;
            Move? bestJumpMove = null;
            var boardState = CaptureThreadSafeBoardState();

            foreach (Move mv in GenerateLegalMovesThreaded(boardState, myTeam))
            {
                var captured = board.ChessPieces[mv.To.x, mv.To.y];
                float captureValue = captured ? PieceValue(captured.type) : 0f;

                bool usesJump = DoesMoveLookLikeJump(mv, boardState);

                if (usesJump && captureValue > gain)
                {
                    gain = captureValue;
                    bestJumpMove = mv;
                }
            }

            board.canJump[myTeam] = prev;

            _strategicJumpMove = bestJumpMove;

            return (gain > 0.1f) ? gain : -1.5f;
        }
        
        private bool DoesMoveLookLikeJump(Move move, BoardState boardState)
        {
            var piece = boardState.Pieces[move.From.x, move.From.y];
            if (piece == null) return false;
            
            if (piece.type == ChessPieceType.Queen || piece.type == ChessPieceType.Rook || piece.type == ChessPieceType.Bishop)
            {
                int dx = Math.Sign(move.To.x - move.From.x);
                int dy = Math.Sign(move.To.y - move.From.y);
                
                int x = move.From.x + dx;
                int y = move.From.y + dy;
                while (x != move.To.x || y != move.To.y)
                {
                    if (boardState.Pieces[x, y] != null)
                        return true; 
                    x += dx;
                    y += dy;
                }
            }
            
            if (piece.type == ChessPieceType.Pawn)
            {
                int distance = Math.Abs(move.To.y - move.From.y);
                if (distance == 2 && boardState.Pieces[move.From.x, move.From.y + (piece.team == GameConstants.WHITE_TEAM ? 1 : -1)] != null)
                    return true;
            }
            
            return false;
        }
        private float ScoreMagicStorm()
        {
            if (_playedMagicStorm)
                return -900;
            
            var boardState = CaptureThreadSafeBoardState();
            float myMat = EvaluateMaterial(boardState);
            
            float totalMaterial = 0f;
            BoardHelper.ForEachPiece(boardState.Pieces, (piece, x, y) =>
            {
                totalMaterial += PieceValue(piece.type);
            });
            float hisMat = totalMaterial - myMat;
            
            return (myMat < hisMat - 1f) ? 2f : -1f;
        }


        private void PlayCard(CardInstance inst)
        {
            var type = System.Type.GetType(inst.Data.effectClassName);
            if (type == null)
                return;

            if (inst.Data.effectClassName == "Cards.Effect.Effects.StrategicJumpEffect")
            {
                _playedStrategicJump = true;
                cardSystem.PlayCard(inst);

                if (_strategicJumpMove.HasValue)
                {
                    var move = _strategicJumpMove.Value;
                    board.MoveTo(move.From.x, move.From.y, move.To.x, move.To.y);
                    _strategicJumpMove = null;
                }
            }
            else if (inst.Data.effectClassName == "Cards.Effect.Effects.MagicStormEffect")
            {
                _playedMagicStorm = true;
                cardSystem.PlayCard(inst);
            }
            else
            {
                cardSystem.PlayCard(inst);
            }
        }
    }
}
