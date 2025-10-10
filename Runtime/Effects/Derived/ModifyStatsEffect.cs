using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class ModifyStatsEffect : EffectBase
    {
        [SerializeReference]
        public StatModifierBase[] modifiers;
    }
}