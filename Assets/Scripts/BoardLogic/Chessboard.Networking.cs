using AI;
using Cards.Runtime;
using ChessGame;
using ChessPieces;
using Net;
using Net.NetMessages;
using UI;
using Unity.Networking.Transport;
using UnityEngine;

namespace BoardLogic
{
    public partial class Chessboard
    {
        private void RegisterEvents()
        {
            GameUI.Instance.SetLocalGame += OnSetLocalGame;
        
            NetUtility.ServerWelcome += OnWelcomeServer;
            NetUtility.ClientWelcome += OnWelcomeClient;
        
            NetUtility.ClientStartGame += OnStartGameClient;
        
            NetUtility.ServerMakeMove += OnMakeMoveServer;
            NetUtility.ClientMakeMove += OnMakeMoveClient;

            NetUtility.ServerRematch += OnRematchServer;
            NetUtility.ClientRematch += OnRematchClient;

            NetUtility.ServerPlayCard += OnPlayCardServer;
            NetUtility.ClientPlayCard += OnPlayCardClient;

        }

        private void UnRegisterEvents()
        {
            GameUI.Instance.SetLocalGame -= OnSetLocalGame;
        
            NetUtility.ServerWelcome -= OnWelcomeServer;
            NetUtility.ClientWelcome -= OnWelcomeClient;
        
            NetUtility.ClientStartGame -= OnStartGameClient;
        
            NetUtility.ServerMakeMove -= OnMakeMoveServer;
            NetUtility.ClientMakeMove -= OnMakeMoveClient;
        
            NetUtility.ServerRematch -= OnRematchServer;
            NetUtility.ClientRematch -= OnRematchClient;
            
            NetUtility.ServerPlayCard -= OnPlayCardServer;
            NetUtility.ClientPlayCard -= OnPlayCardClient;
        }

        
        private void OnSetLocalGame(bool isLocalGame)
        {
            _playerCount = -1;
            myTeam = -1;
            currentTurn = 0;
            localGame = isLocalGame;
        }

        private void OnWelcomeServer(NetMessage msg, NetworkConnection conn)
        {
            if (msg is NetWelcome welcome)
            {
                welcome.AssignedTeam = ++_playerCount;

                Server.Instance.SendToClient(conn, welcome);
            }

            if (_playerCount == 1)
            {
                Server.Instance.Broadcast(new NetStartGame());
            }
        }
    
        
        private void OnWelcomeClient(NetMessage msg)
        {
            if (msg is NetWelcome welcome) 
                myTeam = welcome.AssignedTeam;

            if(localGame && myTeam == GameConstants.WHITE_TEAM)
                Server.Instance.Broadcast(new NetStartGame());
        }

        private void OnStartGameClient(NetMessage msg)
        {
            MoveCameraToCurrentTeam();
            RaiseTurnStarted();
            cardSystem.EnableUI(true);
            cardSystem.StartDecks();
        }

        private void OnMakeMoveServer(NetMessage msg, NetworkConnection conn)
        {
            NetMakeMove makeMove = msg as NetMakeMove;
            Server.Instance.Broadcast(makeMove);
        }
    
        private void OnMakeMoveClient(NetMessage msg)
        {
            if (msg is not NetMakeMove makeMove)
                return;

            if (!localGame && (canMoveEnemyPieces || myTeam != makeMove.TeamId))
            {
                ChessPiece movedPiece = ChessPieces[makeMove.OriginalX, makeMove.OriginalY];
                _availableMoves = FilteredMoves(movedPiece);
                specialMove = movedPiece.GetSpecialMoves(ref ChessPieces, ref MoveList, ref _availableMoves);

                MoveTo(makeMove.OriginalX, makeMove.OriginalY, makeMove.DestinationX, makeMove.DestinationY);
                
                _availableMoves.Clear();
                specialMove = SpecialMove.None;
            }
        }
        
        public void SendMakeMoveMessage(Vector2Int previousPosition, Vector2Int targetPosition)
        {
            NetMakeMove makeMove = new NetMakeMove
            {
                OriginalX = previousPosition.x,
                OriginalY = previousPosition.y,
                DestinationX = targetPosition.x,
                DestinationY = targetPosition.y,
                TeamId = myTeam
            };

            Client.Instance.SendToServer(makeMove);
        }
    
        private void OnRematchServer(NetMessage msg, NetworkConnection conn)
        {
            NetRematch rematch = msg as NetRematch;
            Server.Instance.Broadcast(rematch);
        }
    
        private void OnRematchClient(NetMessage msg)
        {
            if (msg is not NetRematch rematch)
                return;

            _wantRematch[rematch.TeamId] = rematch.WantRematch == 1;

            if(myTeam != rematch.TeamId)
            {
                rematchIndicator.transform.GetChild(rematch.WantRematch).gameObject.SetActive(true);
                if (rematch.WantRematch == 0)
                    rematchButton.interactable = false;
            }
        
            if(_wantRematch[0] && _wantRematch[1])
                GameReset();
        }
        
        private void OnPlayCardServer(NetMessage msg, NetworkConnection conn)
        {
            NetPlayCard playCard = msg as NetPlayCard;
            Server.Instance.Broadcast(playCard);
        }
        
        private void OnPlayCardClient(NetMessage msg)
        {
            if (msg is not NetPlayCard playCard)
                return;
            
            if (!localGame && myTeam != playCard.TeamId)
            {
                var eff = CardRegistry.CreateEffect(playCard.CardId);
                eff?.ResolveRemote(playCard.Payload, cardSystem, this);
                
                if (playCard.Confirmed)                   
                    cardSystem.PlayCardAnimation(CardRegistry.Lookup(playCard.CardId));
            }
        }

        private void ShutDownRelay()
        {
            Client.Instance.Shutdown();
            Server.Instance.Shutdown(); 
            GameReset(true);
            GameUI.Instance.OnLeaveToMenuButton();
        }

    }
}