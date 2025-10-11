using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class ModifyStatsEffect : EffectBase
    {
        public StatModifierSelector[] modifiers;
    }
}