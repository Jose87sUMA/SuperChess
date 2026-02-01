using System;
using Audio;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AudioSettingsMenu : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button languageButton;

        private TMP_Text languageValue;
    private TMP_Text musicLabel;
    private TMP_Text sfxLabel;

        private void Start()
        {
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVol", .2f));
            sfxSlider  .SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVol" , .2f));

            musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            sfxSlider  .onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);

            if (languageButton)
            {
                languageButton.onClick.AddListener(OnLanguageButtonClicked);
                languageValue = languageButton.GetComponentInChildren<TMP_Text>(true);
            }

            if (!languageValue)
            {
                var currentEnglish = LocalizationManager.Instance.GetLanguageDisplayName(Language.English);
                languageValue = FindTextByContent(currentEnglish);
            }

            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
            RegisterStaticLabels();
            OnLanguageChanged(LocalizationManager.Instance.CurrentLanguage);
        }

        private void OnDestroy()
        {
            if (languageButton)
                languageButton.onClick.RemoveListener(OnLanguageButtonClicked);

            if (LocalizationManager.HasInstance)
                LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageButtonClicked()
        {
            LocalizationManager.Instance.CycleLanguage();
        }

        private void OnLanguageChanged(Language language)
        {
            RefreshLanguageSection(language);
            RefreshVolumeLabels();
        }

        private void RefreshLanguageSection(Language language)
        {
            var manager = LocalizationManager.Instance;

            if (languageValue)
                languageValue.text = manager.GetLanguageDisplayName(language);
        }

        private void RegisterStaticLabels()
        {
            foreach (var tmp in GetComponentsInChildren<TMP_Text>(true))
            {
                var normalized = LocalizationKeyMap.Normalize(tmp.text);
                switch (normalized)
                {
                    case "Music":
                    case "Música":
                        musicLabel ??= tmp;
                        break;
                    case "Sound Effects":
                    case "Efectos de sonido":
                    case "Efectos de Sonido":
                        sfxLabel ??= tmp;
                        break;
                }
            }

            RefreshVolumeLabels();
        }

        private void RefreshVolumeLabels()
        {
            if (!LocalizationManager.HasInstance)
                return;

            var manager = LocalizationManager.Instance;
            if (musicLabel)
                manager.RegisterText(musicLabel, "ui.settings.music");
            if (sfxLabel)
                manager.RegisterText(sfxLabel, "ui.settings.sfx");
        }

        private TMP_Text FindTextByContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            foreach (var tmp in GetComponentsInChildren<TMP_Text>(true))
            {
                if (string.Equals(tmp.text?.Trim(), content, StringComparison.OrdinalIgnoreCase))
                    return tmp;
            }

            return null;
        }
    }
}