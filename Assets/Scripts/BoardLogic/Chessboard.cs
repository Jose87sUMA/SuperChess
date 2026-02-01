using System.Collections.Generic;
using ChessPieces;
using ChessGame;
using Net;
using Net.NetMessages;
using UnityEngine;
using UnityEngine.UI;

namespace BoardLogic
{
    public enum SpecialMove
    {
        None = 0,
        EnPassant = 1,
        Castle = 2,
        Promotion = 3
    }

    public partial class Chessboard : MonoBehaviour
    {
        private static readonly (ChessPieceType type, int x, int y)[] InitialSetup = {
            (ChessPieceType.Rook,0,0),(ChessPieceType.Knight,1,0),(ChessPieceType.Bishop,2,0),(ChessPieceType.Queen,3,0),(ChessPieceType.King,4,0),(ChessPieceType.Bishop,5,0),(ChessPieceType.Knight,6,0),(ChessPieceType.Rook,7,0),
            (ChessPieceType.Rook,0,7),(ChessPieceType.Knight,1,7),(ChessPieceType.Bishop,2,7),(ChessPieceType.Queen,3,7),(ChessPieceType.King,4,7),(ChessPieceType.Bishop,5,7),(ChessPieceType.Knight,6,7),(ChessPieceType.Rook,7,7)
        };
    
        public enum TileLayer { Tile, Hover, Highlight }
    
        // LOGIC
        public ChessPiece[,] ChessPieces;
    
        private ChessPiece _currentlyDragging;
        private List<Vector2Int> _availableMoves = new ();
        public List<Vector2Int[]> MoveList = new ();
        public SpecialMove specialMove;

        public int currentTurn;
        private readonly List<ChessPiece> _deadWhites = new ();
        private readonly List<ChessPiece> _deadBlacks = new ();
    

        private GameObject[,] _tiles;
        private Vector2Int _currentHover;

        private Vector3 _bounds;
        private Camera _currentCamera;

        // Net logic
    
        private int _playerCount = -1;
        public int myTeam = -1;
        public bool localGame = true;
        private readonly bool[] _wantRematch = new bool[2];
    
        // Card Logic
        
        private bool _retainTurnOnce;

        // ART

        [Header("Art")]
        [SerializeField] private Material tileMaterial;
        [SerializeField] private Material hoverMaterial;
        [SerializeField] private float tileSize = 1.0f;
        [SerializeField] private float yOffset = 0.2f;
        [SerializeField] private Vector3 boardCenter = Vector3.zero;
    
        [SerializeField] private float deathSize = 0.3f;
        [SerializeField] private float deathSpacing = 0.5f;
        [SerializeField] private float dragOffset = 3f;

    [Header("Input")]
    [SerializeField, Min(0f)] private float escapeHoldDuration = 5f;
    private float _escapeHeldTime;
    private bool _escapeHoldTriggered;
    
        // UI
        [Header("UI")]
        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private Transform rematchIndicator;
        [SerializeField] private Button rematchButton;


        // Prefabs and materials
        [Header("Prefabs and Materials")]
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private Material[] teamMaterials;
    
    
        private void Start()
        {
            currentTurn = GameConstants.WHITE_TEAM;
        
            GenerateAllTiles(tileSize, GameConstants.BOARD_SIZE, GameConstants.BOARD_SIZE);
            SpawnAllPieces();
            PositionAllPieces();

            RegisterEvents();
        }

        private void Update()
        {
            if (!_currentCamera)
            {
                _currentCamera = Camera.main;
                return;
            }

            Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
            {
                Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

                if(_currentHover == -Vector2Int.one)
                {
                    _currentHover = hitPosition;
                    SetTileLayer(hitPosition.x, hitPosition.y, TileLayer.Hover);

                }
                else if (_currentHover != hitPosition)
                {
                    TileLayer layer = ContainsValidMove(ref _availableMoves, _currentHover)
                        ? TileLayer.Highlight
                        : TileLayer.Tile;
                    SetTileLayer(_currentHover.x, _currentHover.y, layer);
                    _currentHover = hitPosition;
                    SetTileLayer(hitPosition.x, hitPosition.y, TileLayer.Hover);

                }
            
                // Selected Piece
                if (Input.GetMouseButtonDown(0))
                {
                    if (ChessPieces[hitPosition.x, hitPosition.y] && dragEnabled)
                    {
                        // Is it our turn?
                        if (ChessPieces[hitPosition.x, hitPosition.y].team == currentTurn && currentTurn == myTeam)
                        {
                            _currentlyDragging = ChessPieces[hitPosition.x, hitPosition.y];

                            _availableMoves = FilteredMoves(_currentlyDragging);
                            specialMove = _currentlyDragging.GetSpecialMoves(ref ChessPieces, ref MoveList, ref _availableMoves);

                            PreventCheck(_currentlyDragging);
                        
                            ToggleHighlights(true);
                        }
                    }
                }

                if (_currentlyDragging && Input.GetMouseButtonUp(0))
                {
                    Vector2Int previousPosition = new Vector2Int(_currentlyDragging.currentX, _currentlyDragging.currentY);
                    if (ContainsValidMove(ref _availableMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                    {
                        MoveTo(previousPosition.x, previousPosition.y, hitPosition.x, hitPosition.y);
                        SendMakeMoveMessage(previousPosition, hitPosition);
                    }
                    else
                    {
                        _currentlyDragging.SetPosition(GetPosition(previousPosition.x, previousPosition.y));
                        ToggleHighlights(false);
                        _availableMoves.Clear();
                        _currentlyDragging = null;  
                    }
                }
            }
            else
            {
                if (_currentHover != -Vector2Int.one)
                {
                    TileLayer layer = ContainsValidMove(ref _availableMoves, _currentHover)
                        ? TileLayer.Highlight
                        : TileLayer.Tile;
                    SetTileLayer(_currentHover.x, _currentHover.y, layer);
                    _currentHover = -Vector2Int.one;
                }

                if (_currentlyDragging && Input.GetMouseButtonUp(0))
                {
                    _currentlyDragging.SetPosition(GetPosition(_currentlyDragging.currentX, _currentlyDragging.currentY));
                
                    ToggleHighlights(false);
                    _availableMoves.Clear();
                    _currentlyDragging = null;
                }
            }

            if (_currentlyDragging)
            {
                Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
                if(horizontalPlane.Raycast(ray, out float distance))
                    _currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }

            HandleEscapeHoldInput();
        }

        private void HandleEscapeHoldInput()
        {
            if (myTeam == -1)
            {
                _escapeHeldTime = 0f;
                _escapeHoldTriggered = false;
                return;
            }

            if (Input.GetKey(KeyCode.Escape))
            {
                _escapeHeldTime += Time.unscaledDeltaTime;
                if (!_escapeHoldTriggered && _escapeHeldTime >= escapeHoldDuration)
                {
                    _escapeHoldTriggered = true;
                    OnExitButton();
                }
            }
            else
            {
                _escapeHeldTime = 0f;
                _escapeHoldTriggered = false;
            }
        }

        private void GameReset(bool complete = false)
        {
            MoveCameraToCurrentTeam();

            // Cards
            cardSystem.StartDecks();
            cardSystem.EnableUI(true);
        
            // Logic fields
            _currentlyDragging = null;
            _availableMoves.Clear();
            MoveList.Clear();
            _wantRematch[0] = _wantRematch[1] = false;
        
            // UI
            rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
            rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

            victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
            victoryScreen.transform.GetChild(1).gameObject.SetActive(false);

            rematchButton.interactable = true;
        
            victoryScreen.SetActive(false);

            // Board and Pieces
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                if (ChessPieces[x, y])
                {
                    Destroy(ChessPieces[x, y].gameObject);
                    ChessPieces[x, y] = null;
                }
        
        
            foreach (ChessPiece piece in _deadWhites)
                Destroy(piece.gameObject);
            
        
            foreach (ChessPiece piece in _deadBlacks)
                Destroy(piece.gameObject);
        
            _deadWhites.Clear();
            _deadBlacks.Clear();
        
            // Re-Initialize
            SpawnAllPieces();
            PositionAllPieces();
            currentTurn = GameConstants.WHITE_TEAM;
            check = false;
            checkmate = false;
            
            if (complete)
            {
                _playerCount = -1;
                myTeam = -1;
                cardSystem.EnableUI(false);
            }
            else
            {
                RaiseTurnStarted(); 
            }
            
        }
    
    }
}