using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Tool
{
    internal class EffectFactoryModule : FactoryModule<EffectBase>
    {
        public EffectFactoryModule(string title, int layer = 1) : base(typeof(EffectBase), title, layer) { }

        protected override EffectBase Create(Type type, Vector2 position, string entryName)
        {
            ScriptableObject createdObject = ScriptableObject.CreateInstance(type);
            Assert.IsNotNull(createdObject);
            
            EffectBase effectBase = createdObject as EffectBase;
            Assert.IsNotNull(effectBase);
            
            effectBase.hideFlags = HideFlags.HideInHierarchy;
            effectBase.name = entryName;
            
            EditorUtility.SetDirty(effectBase);
            return effectBase;
        }
    }
}