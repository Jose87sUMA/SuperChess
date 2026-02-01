using ChessGame;
using Net;
using Net.NetMessages;
using UI;

namespace BoardLogic
{
    public partial class Chessboard
    {
        public bool switchCameraOnPlay = true;
        private void DisplayVictory(int winningTeam)
        {
            _currentlyDragging = null;
            ToggleHighlights(false);
            _availableMoves.Clear();
            cardSystem.EnableUI(false);
            victoryScreen.SetActive(true);

            if (winningTeam == GameConstants.GAME_DRAW)
            {
    
                if (victoryScreen.transform.childCount > 2)
                {
                    victoryScreen.transform.GetChild(2).gameObject.SetActive(true);
                }
            }
            else
            {
                victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
            }
        }

        public void OnRematchButton()
        {
            if (localGame)
            {
                myTeam = GameConstants.WHITE_TEAM;
                GameReset();
            }
            else
            {
                NetRematch rematch = new NetRematch
                {
                    TeamId = myTeam,
                    WantRematch = 1
                };
                Client.Instance.SendToServer(rematch);
            }
        }
    
        public void OnExitButton()
        {
            NetRematch rematch = new NetRematch
            {
                TeamId = myTeam,
                WantRematch = 0
            };
            Client.Instance.SendToServer(rematch);
        
            Invoke(nameof(ShutDownRelay), 1f);
        }
    
        private void MoveCameraToCurrentTeam()
        {
            GameUI.Instance.ChangeCamera(myTeam == GameConstants.WHITE_TEAM ? CameraAngle.WhiteTeam : CameraAngle.BlackTeam);
        }
    }
}