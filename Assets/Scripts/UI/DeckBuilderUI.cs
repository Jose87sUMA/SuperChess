using System.Collections.Generic;
using System.Linq;
using Cards.Data;
using Cards.Runtime;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    public sealed class DeckBuilderUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] RectTransform catalogRoot;
        [SerializeField] RectTransform deckRoot;
        [SerializeField] TMP_Text costLabel;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Button confirmBtn;
        [SerializeField] Button backBtn;
        [SerializeField] GameObject cardButtonPrefab;
        
        private GridLayoutGroup catalogGridLayout;
        private ContentSizeFitter catalogSizeFitter;
        private TMP_Text confirmLabel;
        private TMP_Text backLabel;

        readonly List<CardData> _deck = new();
        CardData _hoveredCard;

        int CostLimit => DeckStorage.Instance.CostLimit;

        private void Start()
        {
            PopulateCatalog();
            
            if (DeckStorage.Instance && DeckStorage.Instance.HasDeck)
            {
                foreach (var data in DeckStorage.Instance.GetDeck())
                    AddDeckEntry(data);
            }
            RefreshUI();

            confirmBtn.onClick.AddListener(OnConfirm);
            backBtn.onClick.AddListener(OnBack);

            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;

            confirmLabel = confirmBtn.GetComponentInChildren<TMP_Text>(true);
            backLabel = backBtn.GetComponentInChildren<TMP_Text>(true);
            RefreshButtons();
        }

        private void OnDestroy()
        {
            if (LocalizationManager.HasInstance)
                LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
        }

        private void PopulateCatalog()
        {
            catalogGridLayout = catalogRoot.GetComponent<GridLayoutGroup>();
            catalogSizeFitter = catalogRoot.GetComponent<ContentSizeFitter>();
            
            var cards = Resources.LoadAll<CardData>("Cards"); 
            foreach (var data in cards)
            {
                var go = Instantiate(cardButtonPrefab, catalogRoot);
                var img = go.GetComponent<Image>();
                img.maskable = false;
                if (img) img.sprite = data.artwork;

                go.GetComponent<Button>()
                  .onClick.AddListener(() => TryAdd(data));

                AddHoverEvents(go, data);

            }
            
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(catalogRoot);
        }

        public void TryAdd(CardData data)
        {
            if (_deck.Sum(c => c.cost) + data.cost > CostLimit) return;
            AddDeckEntry(data);
            RefreshUI();
        }
        
        void AddDeckEntry(CardData data)
        {
            _deck.Add(data);
            var go = Instantiate(cardButtonPrefab, deckRoot);
            var img = go.GetComponent<Image>();
            if (img) img.sprite = data.artwork;

            go.GetComponent<Button>()
              .onClick.AddListener(() =>
              {
                  _deck.Remove(data);
                  Destroy(go);
                  RefreshUI();
              });

            AddHoverEvents(go, data);
        }

        private void RefreshUI()
        {
            var current = _deck.Sum(c => c.cost);
            var manager = LocalizationManager.Instance;
            var template = manager.Get("ui.deck.cost", "Cost: {0}/{1}");
            costLabel.text = string.Format(template, current, CostLimit);
            confirmBtn.interactable = current > 0 && current <= CostLimit;
            RefreshButtons();
        }

        public void OnConfirm()
        {
            DeckStorage.Instance.SetDeck(new List<CardData>(_deck));
            FindObjectOfType<GameUI>().OnLeaveToMenuButton();
        }

        public void OnBack() => FindObjectOfType<GameUI>().OnLeaveToMenuButton();

        private void AddHoverEvents(GameObject go, CardData data)
        {
            var trigger = go.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = go.AddComponent<EventTrigger>();

            var entryEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            entryEnter.callback.AddListener((_) =>
            {
                _hoveredCard = data;
                UpdateHoveredDescription();
            });
            trigger.triggers.Add(entryEnter);

            var entryExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener((_) =>
            {
                _hoveredCard = null;
                descriptionText.text = string.Empty;
                LocalizationManager.Instance.UnregisterText(descriptionText);
            });
            trigger.triggers.Add(entryExit);
        }

        private void UpdateHoveredDescription()
        {
            if (_hoveredCard == null)
                return;

            var manager = LocalizationManager.Instance;
            var key = manager.GetCardDescriptionKey(_hoveredCard);
            manager.RegisterText(descriptionText, key);
            descriptionText.text = manager.GetCardDescription(_hoveredCard);
        }

        private void OnLanguageChanged(Language _)
        {
            RefreshUI();
            UpdateHoveredDescription();
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            var manager = LocalizationManager.Instance;
            if (confirmLabel)
                manager.RegisterText(confirmLabel, "ui.common.confirm");
            if (backLabel)
                manager.RegisterText(backLabel, "ui.common.back");
        }
    }
}
