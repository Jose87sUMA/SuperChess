﻿using System.Collections;
using Cards.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class CardPlayAnimator : MonoBehaviour
    {
        public static CardPlayAnimator Instance { get; private set; }

        [Header("UI")]
        [SerializeField] Image cardImage;
        [SerializeField] CanvasGroup group;

        [Header("Timings")]
        [SerializeField] float fadeIn = .25f;
        [SerializeField] float hold = .60f;
        [SerializeField] float fadeOut = .30f;

        void Awake()
        {
            if (!Instance) Instance = this;
            else { Destroy(gameObject); return; }
            group.gameObject.SetActive(false);
        }

        public void PlayCard(CardData data)
        {
            if (!data || !data.artwork) return;
            StopAllCoroutines();
            StartCoroutine(PlayRoutine(data.artwork));
        }

        IEnumerator PlayRoutine(Sprite art)
        {
            cardImage.sprite = art;
            group.alpha      = 0;
            cardImage.transform.localScale = Vector3.one * .55f;
            group.gameObject.SetActive(true);

            for (float t = 0; t < fadeIn; t += Time.unscaledDeltaTime)
            {
                var k = t / fadeIn;
                group.alpha = k;
                cardImage.transform.localScale =
                    Vector3.Lerp(Vector3.one * .55f, Vector3.one, k);
                yield return null;
            }

            group.alpha = 1;
            yield return new WaitForSecondsRealtime(hold);

            for (float t = 0; t < fadeOut; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1 - t / fadeOut;
                yield return null;
            }

            group.gameObject.SetActive(false);
        }
    }
}
