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
             EffectBase createdEffect = Activator.CreateInstance(type) as EffectBase;
             Assert.IsNotNull(createdEffect, $"Failed to create instance of type {type}");
             createdEffect.name = ObjectNames.NicifyVariableName(entryName);
             return createdEffect;
        }
    }
}