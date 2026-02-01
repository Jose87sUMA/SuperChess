using System;
using System.Collections.Generic;
using System.Reflection;
using Cards.Data;
using UnityEngine;

namespace Cards.Runtime
{
    public static class CardRegistry
    {
        static readonly Dictionary<int, Type> _typeMap = new();
        static readonly Dictionary<int, CardData> _dataMap = new();
        
        static CardRegistry()
        { 
            AutoRegisterResources();
        }
        
        static void AutoRegisterResources()
        { 
            var all = Resources.LoadAll<CardData>("Cards");
            foreach (var d in all) 
                Register(d);
        }
        
        public static void Register(CardData data)
        {
            if (!data) return;

            if (!(Type.GetType(data.effectClassName, false) is { } t))
            {
                Debug.LogError(
                    $"[CardRegistry] Effect class '{data.effectClassName}' not found.");
                return;
            }

            if (_typeMap.TryGetValue(data.netID, out var existing))
            {
                return;
            }

            _typeMap[data.netID] = t;
            _dataMap[data.netID] = data;  
        }

        public static CardEffect CreateEffect(int id)
        {
            if (!_typeMap.TryGetValue(id, out var t))
            {
                Debug.LogError($"[CardRegistry] Unknown cardId {id}");
                return null;
            }
            return (CardEffect) Activator.CreateInstance(t);
        }
        
        public static CardData Lookup(int id)
        {
            if (_dataMap.TryGetValue(id, out var d)) return d;
            Debug.LogError($"[CardRegistry] CardData missing for id {id}");
            return null;
        }
    }
}