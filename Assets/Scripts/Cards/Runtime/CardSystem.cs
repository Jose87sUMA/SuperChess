using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using BoardLogic;
using Cards.Data;
using Cards.Runtime;
using ChessPieces;
using Localization;
using TMPro;
using Net;
using Net.NetMessages;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Cards.Runtime
{
    [RequireComponent(typeof(HandUI))]
    public sealed class CardSystem : MonoBehaviour
    {
        [Header("Starter deck (20 cards)")]
        [SerializeField] private int maxCardsPerHand = 5;
        
        private readonly Deck[] _decks = new Deck[2];
        public readonly Hand[] Hands = { new Hand(), new Hand() };
        private readonly bool[] _openingDrawDone = new bool[2];
        private List<CardData> _deckList = new List<CardData>();

        [Header("Pending-card panel")]
        [SerializeField] private RectTransform pendingPanel;
        [SerializeField] private RectTransform cardAnchor;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button cancelBtn;
    private TMP_Text _confirmLabel;
    private TMP_Text _cancelLabel;

        public event System.Action<CardInstance> CardPlayed;
        
        private HandUI _ui;
        private Chessboard _board;
        public bool currentlyPlayingCard = false;

        private bool IsMyTurn => _board.currentTurn == _board.myTeam;
        private bool _suppressEndTurn;
        
        CardInstance  _queuedCard;
        RectTransform _queuedTransform;
        
        private bool IsAIGame => FindObjectOfType<ChessBot>()?.IsActive == true;
        
        private int GetDisplayedHandTeam()
        {
            return IsAIGame ? 0 : _board.myTeam;
        }

        private void Awake()
        {
            _ui    = GetComponent<HandUI>();
            _board = FindObjectOfType<Chessboard>();

            SetNewDeck();
            
            _board.TurnStarted  += OnTurnStarted;
            _board.OnPieceMoved += OnPieceMoved;
            
            
            foreach (var card in _deckList)
                CardRegistry.Register(card);

            var localization = LocalizationManager.Instance;
            _confirmLabel = confirmBtn ? confirmBtn.GetComponentInChildren<TMP_Text>(true) : null;
            _cancelLabel  = cancelBtn  ? cancelBtn.GetComponentInChildren<TMP_Text>(true)  : null;
            if (_confirmLabel)
                localization.RegisterText(_confirmLabel, "ui.common.confirm");
            if (_cancelLabel)
                localization.RegisterText(_cancelLabel, "ui.common.cancel");
        }

        public void SetNewDeck()
        {
            if (DeckStorage.Instance && DeckStorage.Instance.HasDeck)
                _deckList = new List<CardData>(DeckStorage.Instance.GetDeck());
            
            StartDecks();
        }        
        
        public void StartDecks()
        {
            
            if (IsAIGame)
            {
                var aiDeck = CreateAIPredefinedDeck();
                
                _decks[0] = new Deck(_deckList);
                _decks[1] = new Deck(aiDeck);
            }
            else
            {
                _decks[0] = new Deck(_deckList);
                _decks[1] = new Deck(_deckList);
            }
            
            Hands[0] = new Hand();
            Hands[1] = new Hand();
            _openingDrawDone[0] = false;
            _openingDrawDone[1] = false;
            
            pendingPanel.gameObject.SetActive(false);
        }
          private List<CardData> CreateAIPredefinedDeck()
        {
            var aiDeck = new List<CardData>();
            
            var magicStorm = CardRegistry.Lookup(1);
            var strategicJump = CardRegistry.Lookup(2);
            var quickStep = CardRegistry.Lookup(3);  
  
            if (strategicJump != null)
            {
                aiDeck.Add(strategicJump);
                aiDeck.Add(strategicJump);
            }
            
            if (quickStep != null)
            {
                aiDeck.Add(quickStep);
                aiDeck.Add(quickStep);
            }
            
            if (magicStorm != null)
            {
                aiDeck.Add(magicStorm);
                aiDeck.Add(magicStorm);
            }
            
            return aiDeck;
        }

        private void OnTurnStarted(int playingTeam)
        {
            if (_board.localGame || playingTeam != _board.myTeam)
                ClearPendingCard(refreshHand: false);

            if (!_openingDrawDone[playingTeam])
            {
                Draw(playingTeam, 2);
                _openingDrawDone[playingTeam] = true;
            }

            Draw(playingTeam, 1);

            if (IsAIGame || IsMyTurn)
                _ui.Sync(Hands[GetDisplayedHandTeam()]);
        }

        private void OnPieceMoved(ChessPiece _)
        {
            if (!IsMyTurn) return;

            if (_suppressEndTurn)
            {
                _suppressEndTurn = false;
            }
        }
        
        public void SuppressNextAutoEndTurn() => _suppressEndTurn = true;

        public void QueueCard(CardInstance card, Transform cardTransform)
        {
            if (!IsMyTurn || card == null || currentlyPlayingCard) return;
            
            ClearPendingCard(refreshHand: false);

            _queuedCard = card;
            _queuedTransform = cardTransform as RectTransform;

            _queuedTransform.SetParent(cardAnchor,false);
            _queuedTransform.anchorMin = _queuedTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _queuedTransform.anchoredPosition = Vector2.zero;
            
            pendingPanel.gameObject.SetActive(true);

            confirmBtn.onClick.RemoveAllListeners();
            cancelBtn .onClick.RemoveAllListeners();
            
            confirmBtn.onClick.AddListener(ConfirmQueued);
            cancelBtn .onClick.AddListener(CancelQueued);
        }

        private void ConfirmQueued()
        {
            pendingPanel.gameObject.SetActive(false);
            _queuedTransform.SetParent(_ui.cardRoot,false); 
            PlayCard(_queuedCard);
            _queuedCard = null;
        }
        
        private void CancelQueued()
        {
            pendingPanel.gameObject.SetActive(false);
            _queuedTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _queuedTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _queuedTransform.anchoredPosition = Vector2.zero;

            _queuedTransform.SetParent(_ui.cardRoot, false);
            _queuedCard = null;
            _ui.Sync(Hands[GetDisplayedHandTeam()]);
        }
        
        private void ClearPendingCard(bool refreshHand = true)
        {
            if (_queuedCard == null) return;

            pendingPanel.gameObject.SetActive(false);

            if (_queuedTransform)
            {
                _queuedTransform.SetParent(_ui.cardRoot, false);
                _queuedTransform.anchorMin = new Vector2(0.5f, 0.5f);
                _queuedTransform.anchorMax = new Vector2(0.5f, 0.5f);
                _queuedTransform.anchoredPosition = Vector2.zero;
            }

            _queuedCard = null;
            _queuedTransform = null;

            if (refreshHand)
                _ui.Sync(Hands[GetDisplayedHandTeam()]);
        }

        public void PlayCard(CardInstance inst)
        {
            var type = Type.GetType(inst.Data.effectClassName);
            if (type == null) { Debug.LogError("Bad effect class"); return; }

            var effect = (CardEffect)Activator.CreateInstance(type);

            SendMessage(inst, effect, false);
            effect.Resolve(inst, this, _board);

            if (!effect.DeferredConfirmation)
                SendMessage(inst, effect, true);

            Hands[_board.myTeam].Remove(inst);
            _ui.Sync(Hands[GetDisplayedHandTeam()]);

            CardPlayed?.Invoke(inst);
        }

        public void SendMessage(CardInstance inst, CardEffect effect, bool confirmed)
        {
            if (!_board.localGame && Client.Instance)
            {
                var pc = new NetPlayCard
                {
                    CardId = inst.Data.netID,
                    PayloadLen = (byte)effect.SerializePayload().Length,
                    TeamId = _board.myTeam,
                    Confirmed  = confirmed,
                    Payload = effect.SerializePayload()
                };
                
                Client.Instance.SendToServer(pc);
            }
            
            if (confirmed) PlayCardAnimation(inst.Data);
        }

        internal void PlayCardAnimation(CardData data)
        {
            CardPlayAnimator.Instance?.PlayCard(data);
        }

        private void Draw(int team, int n){
            while (Hands[team].Cards.Count < maxCardsPerHand && n > 0)
            {
                CardInstance c = _decks[team].Draw(); 
                if(c != null)
                    Hands[team].Add(c);
                --n;
            } 
        }
        public void RefundCard(CardInstance inst)
        {
            Hands[_board.myTeam].Add(inst);
            _ui.Sync(Hands[GetDisplayedHandTeam()]);
        }

        public void EnableUI(bool b)
        {
            _ui.gameObject.SetActive(b);
        }
    }
}
