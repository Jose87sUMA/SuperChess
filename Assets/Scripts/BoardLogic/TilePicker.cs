using System.Collections.Generic;
using Cards.Runtime;
using UnityEngine;

namespace BoardLogic
{

    [RequireComponent(typeof(Chessboard))]
    public sealed class TilePicker : MonoBehaviour
    {
        static readonly List<ITileClickListener> _listeners = new();

        public static void Register  (ITileClickListener l) => _listeners.Add(l);
        public static void Unregister(ITileClickListener l) => _listeners.Remove(l);

        Chessboard _board;

        void Awake() => _board = GetComponent<Chessboard>();

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;

            Vector2Int square = _board.LookupTileIndex(hit.transform.gameObject);
            if (square.x < 0) return;                       
            foreach (var l in _listeners.ToArray())
            {
                var sys = _board.GetComponentInChildren<CardSystem>();  
                l.OnBoardClick(square, _board);                        
            }

        }
    }

    public interface ITileClickListener
    {
        void OnBoardClick(Vector2Int square, Chessboard board);
    }
}