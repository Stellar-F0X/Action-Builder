using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ActionBuilder.Runtime
{
    internal static class StatModifierPool
    {
        private readonly static Dictionary<Type, List<StatModifierBase>> _ModifierPool = new Dictionary<Type, List<StatModifierBase>>();

        public static StatModifierBase Get<T>() where T : StatModifierBase, new() 
        {
            Type type = typeof(T);
            
            if (_ModifierPool.TryGetValue(type, out List<StatModifierBase> modifiers) && modifiers.Count > 0)
            {
                StatModifierBase modifier = modifiers[modifiers.Count - 1];
                modifiers.RemoveAt(modifiers.Count - 1);
                return modifier;
            }

            StatModifierBase newModifier = new T();
            _ModifierPool.TryGetValue(type, out List<StatModifierBase> modifierList);
            
            if (modifierList is null)
            {
                modifierList = new List<StatModifierBase>();
                _ModifierPool[type] = modifierList;
            }
            
            modifierList.Add(newModifier);
            return newModifier;
        }
        

        public static void Release([NotNull] StatModifierBase modifier)
        {
            Type type = modifier.GetType();
            modifier.Reset();

            if (_ModifierPool.TryGetValue(type, out List<StatModifierBase> modifiers) == false)
            {
                modifiers = new List<StatModifierBase>();
                _ModifierPool[type] = modifiers;
            }

            if (modifiers.Contains(modifier) == false)
            {
                modifiers.Add(modifier);
            }
        }
        
        
        public static void ClearPool()
        {
            _ModifierPool.Clear();
        }
        
        
        public static void ClearPool<TSpecific>() where TSpecific : StatModifierBase
        {
            Type type = typeof(TSpecific);
            _ModifierPool.Remove(type);
        }
    }
}