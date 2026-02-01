using System;
using AI;
using ChessGame;
using Localization;
using Net;
using TMPro;
using UnityEngine;

namespace UI
{
    public enum CameraAngle
    {
        Menu = 0,
        WhiteTeam = 1,
        BlackTeam = 2,
    }

    public class GameUI : MonoBehaviour
    {
        private static readonly int MainMenu = Animator.StringToHash("MainMenu");
        private static readonly int OnlineMenu = Animator.StringToHash("OnlineMenu");
        private static readonly int HostMenu = Animator.StringToHash("HostMenu");
        private static readonly int InGame = Animator.StringToHash("InGame");
        private static readonly int AIMenu = Animator.StringToHash("AIMenu");
        private static readonly int DeckBuilder = Animator.StringToHash("DeckBuilder");
        private static readonly int SettingsMenu = Animator.StringToHash("SettingsMenu");

        public static GameUI Instance {set; get;}
        public Server server;
        public Client client;
    
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private TMP_InputField addressInput;
        [SerializeField] private GameObject[] cameraAngles;
        [SerializeField] private ChessBot chessBot;
        
        public Action<bool> SetLocalGame;

    private TMP_Text aiPromptLabel;
    private TMP_Text aiEasyLabel;
    private TMP_Text aiMediumLabel;
    private TMP_Text aiHardLabel;

        private void Awake()
        {
            Instance = this;
            if (LocalizationManager.HasInstance)
                LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            if (LocalizationManager.HasInstance)
                LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
        }

        private void Start()
        {
            ApplyLocalization();
        }
    
        public void ChangeCamera(CameraAngle index)
        {
            for (int i = 0; i < cameraAngles.Length; i++)
            {
                cameraAngles[i].SetActive((int)index == i);
            }

            if (index == CameraAngle.Menu)
                menuAnimator.SetTrigger(MainMenu);
            else
                menuAnimator.SetTrigger(InGame);
        }
    
        public void OnLocalGameButton()
        {
            chessBot.SetActive(false);
            menuAnimator.SetTrigger(InGame);
            SetLocalGame?.Invoke(true);
            server.Init(GameConstants.DEFAULT_PORT);
            client.Init("127.0.0.1", GameConstants.DEFAULT_PORT);
        }
    
        public void OnOnlineGameButton()
        {
            menuAnimator.SetTrigger(OnlineMenu);
        }
    
        public void OnOnlineHostButton()
        {
            chessBot.SetActive(false);
            menuAnimator.SetTrigger(HostMenu);
            SetLocalGame?.Invoke(false);
            server.Init(GameConstants.DEFAULT_PORT);
            client.Init("127.0.0.1", GameConstants.DEFAULT_PORT);
        }

        public void OnOnlineConnectButton()
        {
            chessBot.SetActive(false);
            SetLocalGame?.Invoke(false);
            client.Init(addressInput.text, GameConstants.DEFAULT_PORT);
        }
    
        public void OnOnlineBackButton()
        {
            menuAnimator.SetTrigger(MainMenu);
        }
    
        public void OnHostingBackButton()
        {
            menuAnimator.SetTrigger(OnlineMenu);
            server.Shutdown();
            client.Shutdown();
        }

        public void OnLeaveToMenuButton()
        {
            ChangeCamera(CameraAngle.Menu);
            menuAnimator.SetTrigger(MainMenu);
        }
        
        public void OnVsAIButton()
        {
            menuAnimator.SetTrigger(AIMenu);
            ApplyLocalization();
        }
        
        public void OnAIEasyButton() => StartAIGame(1);
        public void OnAIMediumButton() => StartAIGame(2);
        public void OnAIHardButton() => StartAIGame(3);

        private void StartAIGame(int depth)
        {
            chessBot.ConfigureAI(depth);
            menuAnimator.SetTrigger(InGame);
            SetLocalGame?.Invoke(true);
            server.Init(GameConstants.DEFAULT_PORT);
            client.Init("127.0.0.1", GameConstants.DEFAULT_PORT);
        }

        public void OnDeckBuilderButton()
        {
            menuAnimator.SetTrigger(DeckBuilder);
        }
        
        public void OnSettingsButton()
        {
            menuAnimator.SetTrigger(SettingsMenu);
        }
        
        public void OnCloseGameButton()
        {
            Application.Quit();
        }

        private void OnLanguageChanged(Language _)
        {
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (!LocalizationManager.HasInstance)
                return;

            var manager = LocalizationManager.Instance;

            EnsureAiLabels();

            if (aiPromptLabel)
                manager.RegisterText(aiPromptLabel, "ui.menu.ai.prompt");
            if (aiEasyLabel)
                manager.RegisterText(aiEasyLabel, "ui.menu.ai.beginner");
            if (aiMediumLabel)
                manager.RegisterText(aiMediumLabel, "ui.menu.ai.normal");
            if (aiHardLabel)
                manager.RegisterText(aiHardLabel, "ui.menu.ai.expert");
        }

        private void EnsureAiLabels()
        {
            if (aiPromptLabel && aiEasyLabel && aiMediumLabel && aiHardLabel)
                return;

            foreach (var text in GetComponentsInChildren<TMP_Text>(true))
            {
                var normalized = LocalizationKeyMap.Normalize(text.text);
                switch (normalized)
                {
                    case "Escoge la dificultad":
                    case "Choose the difficulty":
                        aiPromptLabel ??= text;
                        break;
                    case "Principiante":
                    case "Beginner":
                        aiEasyLabel ??= text;
                        break;
                    case "Normal":
                        aiMediumLabel ??= text;
                        break;
                    case "Experto":
                    case "Expert":
                        aiHardLabel ??= text;
                        break;
                }
            }
        }


    }
}