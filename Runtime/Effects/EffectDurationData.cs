using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct EffectDurationData
    {
        [Min(0)]
        public float duration;
        
        [Min(1)]
        public int maxApplicationCount;
    }
}