using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct StatModifierSelector
    {
        public string key;
        public float value;
        
        [Min(0)]
        public int priority;
        public StatModifierType modifierType;
    }
}