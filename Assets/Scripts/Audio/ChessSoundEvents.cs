using BoardLogic;
using Cards.Runtime;
using UnityEngine;

namespace Audio
{
    public class ChessSoundEvents : MonoBehaviour
    {
        [SerializeField] private Chessboard board;
        [SerializeField] private CardSystem cardSystem;

        private void Awake()
        {
            board ??= FindObjectOfType<Chessboard>();
            cardSystem ??= FindObjectOfType<CardSystem>();

            board.OnPieceMoved += _ => AudioManager.Instance.PlaySfx("move");
            board.TurnStarted += OnTurnStarted;
            board.TurnEnded += OnTurnEnded;

            cardSystem.CardPlayed += _  => AudioManager.Instance.PlaySfx("playCard");
        }

        private void OnTurnStarted(int turn)
        {   if (board.MoveList.Count == 0) AudioManager.Instance.PlaySfx("startGame"); }

        private void OnTurnEnded(int turn)
        {
            if (board.checkmate)
                AudioManager.Instance.PlaySfx("checkmate");
            else if (board.check)
                AudioManager.Instance.PlaySfx("check");
        }
    }
}