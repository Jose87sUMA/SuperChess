using System.Collections.Generic;
using System.Linq;
using Cards.Data;
using UnityEngine;

namespace Cards.Runtime
{
    public sealed class DeckStorage : MonoBehaviour
    {
        public static DeckStorage Instance { get; private set; }

        [SerializeField] private int costLimit = 60;
        [SerializeField] private List<CardData> fallbackDeck = new();
        [SerializeField] private CardSystem cardSystem;

        List<CardData> _chosen = null;
        const string kPrefsKey = "SavedDeck"; 

        private void Awake()
        {
            if (!Instance) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
            
            LoadDeckFromPrefs();  
        }

        
        public int  CostLimit   => costLimit;
        public bool HasDeck => _chosen is { Count: >0 };

        public int  CurrentCost => _chosen?.Sum(c => c.cost) ?? 0;

        public IReadOnlyList<CardData> GetDeck() => _chosen ?? fallbackDeck;

        public void SetDeck(List<CardData> cards)
        {
            _chosen = cards;
            SaveDeckToPrefs();
            cardSystem.SetNewDeck();
        }
        
        void SaveDeckToPrefs()
        {
            if (_chosen == null || _chosen.Count == 0) return;
                    var ids = string.Join(",", _chosen.Select(c => c.netID));
                    
            PlayerPrefs.SetString(kPrefsKey, ids);
            PlayerPrefs.Save();
        }
        void LoadDeckFromPrefs()
        {
            if (!PlayerPrefs.HasKey(kPrefsKey)) return;

            var raw = PlayerPrefs.GetString(kPrefsKey);
            var ids = raw.Split(',').Select(s => int.TryParse(s, out var v) ? v : -1).Where(v => v >= 0);

            var list = new List<CardData>();
            foreach (var id in ids)
            {
                var cd = CardRegistry.Lookup(id);
                if (cd != null) list.Add(cd);
            }

            if (list.Count > 0) _chosen = list;
        }
    }
}