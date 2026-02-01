using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Localization
{
    public enum Language
    {
        English = 0,
        Spanish = 1
    }

    [Serializable]
    internal sealed class LocalizationEntry
    {
        public string key;
        public string english;
        public string spanish;
    }

    [Serializable]
    internal sealed class LocalizationDataset
    {
        public LocalizationEntry[] entries;
    }

    /// <summary>
    /// Centralized localization provider. Loads translations from JSON and keeps
    /// UI texts in sync with the current language.
    /// </summary>
    public sealed class LocalizationManager : MonoBehaviour
    {
        private const string PlayerPrefsKey = "Localization.Language";
        private const string ResourcePath = "Localization/strings";

        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateSingleton();
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        private readonly Dictionary<string, LocalizationEntry> _entryMap = new();
        private readonly Dictionary<int, string> _componentKeys = new();
        private readonly Dictionary<int, TMP_Text> _tmpTexts = new();
        private readonly Dictionary<int, Text> _uguiTexts = new();
        private readonly HashSet<string> _missingKeys = new();

        private Language _currentLanguage = Language.English;
        public Language CurrentLanguage => _currentLanguage;

        public event Action<Language> LanguageChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureLoaded()
        {
            if (_instance == null)
                CreateSingleton();
        }

        private static void CreateSingleton()
        {
            var existing = FindObjectOfType<LocalizationManager>();
            if (existing)
            {
                _instance = existing;
                return;
            }

            var go = new GameObject(nameof(LocalizationManager));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<LocalizationManager>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadDataset();
            SceneManager.sceneLoaded += OnSceneLoaded;

            var saved = PlayerPrefs.GetInt(PlayerPrefsKey, (int)Language.English);
            SetLanguage((Language)Mathf.Clamp(saved, 0, Enum.GetValues(typeof(Language)).Length - 1), true);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                _instance = null;
            }
        }

        private void LoadDataset()
        {
            _entryMap.Clear();
            var asset = Resources.Load<TextAsset>(ResourcePath);
            if (!asset)
            {
                Debug.LogError($"[Localization] Missing resource at Resources/{ResourcePath}.json");
                return;
            }

            try
            {
                var dataset = JsonUtility.FromJson<LocalizationDataset>(asset.text);
                if (dataset?.entries == null)
                {
                    Debug.LogError("[Localization] Invalid localization dataset");
                    return;
                }

                foreach (var entry in dataset.entries)
                {
                    if (string.IsNullOrEmpty(entry.key))
                        continue;

                    if (_entryMap.ContainsKey(entry.key))
                        Debug.LogWarning($"[Localization] Duplicate key '{entry.key}'");

                    _entryMap[entry.key] = entry;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Localization] Failed to parse dataset: {ex.Message}");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyStaticLocalizations();
        }

        public void CycleLanguage()
        {
            var next = _currentLanguage == Language.English ? Language.Spanish : Language.English;
            SetLanguage(next);
        }

        public void SetLanguage(Language language, bool skipPersist = false)
        {
            if (_currentLanguage == language)
            {
                ApplyStaticLocalizations();
                ApplyRegisteredTexts();
                LanguageChanged?.Invoke(_currentLanguage);
                return;
            }

            _currentLanguage = language;
            if (!skipPersist)
            {
                PlayerPrefs.SetInt(PlayerPrefsKey, (int)_currentLanguage);
                PlayerPrefs.Save();
            }

            ApplyStaticLocalizations();
            ApplyRegisteredTexts();
            LanguageChanged?.Invoke(_currentLanguage);
        }

        public string Get(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key))
                return fallback ?? string.Empty;

            if (_entryMap.TryGetValue(key, out var entry))
            {
                return _currentLanguage switch
                {
                    Language.English => string.IsNullOrEmpty(entry.english) ? fallback ?? entry.key : entry.english,
                    Language.Spanish => string.IsNullOrEmpty(entry.spanish) ? fallback ?? entry.key : entry.spanish,
                    _ => fallback ?? entry.key
                };
            }

            if (ShouldLogMissingKey(key))
            {
                if (_missingKeys.Add(key))
                    Debug.LogWarning($"[Localization] Missing key '{key}'");
            }
            return fallback ?? key;
        }

        public string GetLanguageDisplayName(Language language)
        {
            return language switch
            {
                Language.English => Get("language.display.english", "English"),
                Language.Spanish => Get("language.display.spanish", "Spanish"),
                _ => language.ToString()
            };
        }

        public string GetCardName(CardData data)
        {
            if (!data) return string.Empty;
            var fallback = string.IsNullOrEmpty(data.displayName) ? data.name : data.displayName;
            return Get(GetCardNameKey(data), fallback);
        }

        public string GetCardDescription(CardData data)
        {
            if (!data) return string.Empty;
            var fallback = data.description;
            return Get(GetCardDescriptionKey(data), fallback);
        }

        public string GetCardNameKey(CardData data)
        {
            if (!data) return string.Empty;
            var baseId = string.IsNullOrEmpty(data.id) ? data.name : data.id;
            return $"card.{baseId}.name";
        }

        public string GetCardDescriptionKey(CardData data)
        {
            if (!data) return string.Empty;
            var baseId = string.IsNullOrEmpty(data.id) ? data.name : data.id;
            return $"card.{baseId}.description";
        }

        public void RegisterText(TMP_Text text, string key)
        {
            if (!text) return;
            var id = text.GetInstanceID();
            _tmpTexts[id] = text;
            _componentKeys[id] = key;
            UpdateText(id);
        }

        public void RegisterText(Text text, string key)
        {
            if (!text) return;
            var id = text.GetInstanceID();
            _uguiTexts[id] = text;
            _componentKeys[id] = key;
            UpdateText(id);
        }

        public void UnregisterText(TMP_Text text)
        {
            if (!text) return;
            var id = text.GetInstanceID();
            _tmpTexts.Remove(id);
            _componentKeys.Remove(id);
        }

        public void UnregisterText(Text text)
        {
            if (!text) return;
            var id = text.GetInstanceID();
            _uguiTexts.Remove(id);
            _componentKeys.Remove(id);
        }

        public void ApplyStaticLocalizations()
        {
            foreach (var text in FindObjectsOfType<TMP_Text>(true))
            {
                var id = text.GetInstanceID();
                if (!_componentKeys.ContainsKey(id))
                {
                    var key = ResolveKey(text.text);
                    if (string.IsNullOrEmpty(key))
                        continue;

                    _tmpTexts[id] = text;
                    _componentKeys[id] = key;
                }
                UpdateText(id);
            }

            foreach (var text in FindObjectsOfType<Text>(true))
            {
                var id = text.GetInstanceID();
                if (!_componentKeys.ContainsKey(id))
                {
                    var key = ResolveKey(text.text);
                    if (string.IsNullOrEmpty(key))
                        continue;

                    _uguiTexts[id] = text;
                    _componentKeys[id] = key;
                }
                UpdateText(id);
            }
        }

        private void ApplyRegisteredTexts()
        {
            foreach (var id in _componentKeys.Keys.ToList())
                UpdateText(id);
        }

        private void UpdateText(int id)
        {
            if (!_componentKeys.TryGetValue(id, out var key))
                return;

            if (_tmpTexts.TryGetValue(id, out var tmpText))
            {
                if (tmpText)
                {
                    tmpText.text = Get(key, tmpText.text);
                }
                else
                {
                    _tmpTexts.Remove(id);
                    _componentKeys.Remove(id);
                }
                return;
            }

            if (_uguiTexts.TryGetValue(id, out var uiText))
            {
                if (uiText)
                {
                    uiText.text = Get(key, uiText.text);
                }
                else
                {
                    _uguiTexts.Remove(id);
                    _componentKeys.Remove(id);
                }
            }
        }

        private string ResolveKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var normalized = LocalizationKeyMap.Normalize(raw);
            if (LocalizationKeyMap.TryGetKey(normalized, out var mapped))
                return mapped;

            return normalized;
        }

        private static bool ShouldLogMissingKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (key.StartsWith("card.", StringComparison.OrdinalIgnoreCase))
                return false;

            foreach (var c in key)
            {
                if (char.IsLetter(c))
                    return true;
            }
            return false;
        }
    }
}
