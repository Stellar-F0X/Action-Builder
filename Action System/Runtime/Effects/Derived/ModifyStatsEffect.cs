using StatSystem.Runtime;
using UnityEngine;

namespace ActionSystem.Runtime
{
    public class ModifyStatsEffect : EffectBase
    {
        [SerializeReference]
        public StatModifierBase[] modifiers;
    }
}