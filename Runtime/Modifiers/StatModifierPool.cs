using System.Collections.Generic;

namespace StatController.Runtime
{
    internal static class StatModifierPool<T> where T : StatModifierBase
    {
        private readonly static HashSet<StatModifierBase> _ModifierPool = new HashSet<StatModifierBase>();
    }
}