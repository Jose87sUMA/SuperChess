using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Cards.Runtime
{
    public sealed class HandUI : MonoBehaviour
    {
        [SerializeField] public Transform  cardRoot;
        [SerializeField] GameObject        cardPrefab;
        [SerializeField] TMP_Text          messageText;

        [SerializeField] float displayDuration = 5f;
        [SerializeField] float fadeDuration    = 1f;

        CardSystem _system;
        Color _originalColor;
        Coroutine _infoCoroutine;
        Hand _lastHand;

        void Awake()
        {
            _system = GetComponentInParent<CardSystem>();
            _originalColor = messageText.color;
            messageText.color = new Color(
                _originalColor.r,
                _originalColor.g,
                _originalColor.b,
                0f
            );
        }

        void OnEnable()
        {
            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        }

        void OnDisable()
        {
            if (LocalizationManager.HasInstance)
                LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
        }

        public void Sync(Hand hand)
        {
            _lastHand = hand;

            foreach (Transform c in cardRoot)
                Destroy(c.gameObject);

            if (hand == null)
                return;

            foreach (var ci in hand.Cards)
            {
                var g = Instantiate(cardPrefab, cardRoot);

                var img = g.GetComponent<Image>();
                if (img) img.sprite = ci.Data.artwork;

                var txt = g.GetComponentInChildren<TMP_Text>(true);
                if (txt)
                {
                    var manager = LocalizationManager.Instance;
                    txt.SetText(manager.GetCardName(ci.Data));
                    manager.RegisterText(txt, manager.GetCardNameKey(ci.Data));
                }

                var btn = g.GetComponent<Button>();
                if (btn) btn.onClick.AddListener(() => _system.QueueCard(ci, g.transform));
            }
        }

        void OnLanguageChanged(Language _)
        {
            if (_lastHand != null)
                Sync(_lastHand);
        }

        public void ShowInfo(string msg)
        {
            if (_infoCoroutine != null)
                StopCoroutine(_infoCoroutine);

            _infoCoroutine = StartCoroutine(ShowInfoCoroutine(msg));
        }

        IEnumerator ShowInfoCoroutine(string msg)
        {
            messageText.text = msg;
            messageText.color = new Color(
                _originalColor.r,
                _originalColor.g,
                _originalColor.b,
                _originalColor.a
            );

            yield return new WaitForSeconds(displayDuration);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                float t = elapsed / fadeDuration;
                float alpha = Mathf.Lerp(_originalColor.a, 0f, t);
                messageText.color = new Color(
                    _originalColor.r,
                    _originalColor.g,
                    _originalColor.b,
                    alpha
                );
                elapsed += Time.deltaTime;
                yield return null;
            }

            messageText.color = new Color(
                _originalColor.r,
                _originalColor.g,
                _originalColor.b,
                0f
            );
            messageText.text = string.Empty;
            _infoCoroutine = null;
        }
    }
}
