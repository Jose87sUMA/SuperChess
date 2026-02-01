using UnityEngine;
using Cards.Data;

namespace Cards.Data
{
    [CreateAssetMenu(menuName = "Chess-Cards/Card")]
    public sealed class CardData : ScriptableObject
    {
        [Header("Meta")]
        public string   id;
        public int netID;
        public string   displayName;
        [TextArea] public string description;
        public Sprite artwork;

        [Header("Gameplay")]
        public CardType cardType;
        [Range(0,30)]  public int cost;
        public string effectClassName;
    }
}