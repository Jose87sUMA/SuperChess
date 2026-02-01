using ChessGame;
using ChessPieces;
using UnityEngine;

namespace BoardLogic
{
    public partial class Chessboard
    {
        private void GenerateAllTiles(float localTileSize, int tileCountX, int tileCountY)
        {
            yOffset += transform.position.y;
            _bounds = new Vector3((tileCountX / 2) * localTileSize, 0, (tileCountX / 2) * localTileSize) + boardCenter;
        
            _tiles = new GameObject[tileCountX, tileCountY];
            for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                _tiles[x,y] = GenerateSingleTile(localTileSize, x, y);

        }

        private GameObject GenerateSingleTile(float localTileSize, int x, int y)
        {
            GameObject tileObject = new GameObject($"X:{x}, Y:{y}");
            tileObject.transform.parent = transform;

            Mesh mesh = new Mesh();
            tileObject.AddComponent<MeshFilter>().mesh = mesh;
            tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(x * localTileSize, yOffset, y * localTileSize) - _bounds;
            vertices[1] = new Vector3(x * localTileSize, yOffset, (y + 1) * localTileSize) - _bounds;
            vertices[2] = new Vector3((x + 1) * localTileSize, yOffset, y * localTileSize) - _bounds;
            vertices[3] = new Vector3((x + 1) * localTileSize, yOffset, (y + 1) * localTileSize) - _bounds;

            int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();

            tileObject.AddComponent<BoxCollider>();

            tileObject.layer = LayerMask.NameToLayer("Tile");

            return tileObject;

        }
    
        private void SpawnAllPieces()
        {
            ChessPieces = new ChessPiece[GameConstants.BOARD_SIZE, GameConstants.BOARD_SIZE];

            foreach (var (type,x,y) in InitialSetup)
            {
                int team = y == 0 ? GameConstants.WHITE_TEAM : GameConstants.BLACK_TEAM;
                ChessPieces[x,y] = SpawnSinglePiece(type, team);
            }

            for (int x = 0; x < GameConstants.BOARD_SIZE; ++x)
            {
                ChessPieces[x,1] = SpawnSinglePiece(ChessPieceType.Pawn, GameConstants.WHITE_TEAM);
                ChessPieces[x,6] = SpawnSinglePiece(ChessPieceType.Pawn, GameConstants.BLACK_TEAM);
            }
        }
    
        public ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
        {
            ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        
            cp.type = type;
            cp.team = team;
            cp.GetComponent<MeshRenderer>().material = teamMaterials[team];
        
            return cp;
        }
    
        private void PositionAllPieces()
        {
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                if (ChessPieces[x, y])
                    PositionSinglePiece(x, y, true);
        }

        private void PositionSinglePiece(int x, int y, bool force = false)
        {
            ChessPieces[x, y].currentX = x;
            ChessPieces[x, y].currentY = y;
            ChessPieces[x, y].SetPosition(GetPosition(x, y), force);
        }

        public Vector3 GetPosition(int x, int y)
        {
            return new Vector3(x * tileSize, yOffset, y * tileSize);
        }
    
        public void SetTileLayer(int x, int y, TileLayer layer)
        {
            _tiles[x, y].layer = LayerMask.NameToLayer(layer.ToString());
        }
        public void ToggleHighlights(bool on) {
            foreach (Vector2Int m in _availableMoves)
                SetTileLayer(m.x, m.y, on ? TileLayer.Highlight : TileLayer.Tile);
            
        }
    
        public Vector2Int LookupTileIndex(GameObject hitInfo) 
        {
            for (int x = 0; x < GameConstants.BOARD_SIZE; x++)
            for (int y = 0; y < GameConstants.BOARD_SIZE; y++)
                if (_tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

            return -Vector2Int.one;
        } 
        
        public bool IsSquareEmpty(Vector2Int p) => !ChessPieces[p.x, p.y];

        public bool IsSquareOccupiedByTeam(Vector2Int p, int team) =>
            ChessPieces[p.x, p.y] &&
            ChessPieces[p.x, p.y].team == team;
    }
}
