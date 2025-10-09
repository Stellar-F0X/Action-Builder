using ActionBuilder.Runtime;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public class ModifyStatsEffect : EffectBase
    {
        [SerializeReference]
        public StatModifierBase[] modifiers;
    }
}