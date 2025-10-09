using System;
using ActionBuilder.Runtime;
using UnityEngine;

namespace ActionBuilder.Tool
{
    internal class EffectFactoryModule : FactoryModule<EffectBase> 
    {
        public EffectFactoryModule(string title, int layer = 1) : base(typeof(EffectBase), title, layer) { }
        
        protected override EffectBase Create(Type type, Vector2 position, string entryName)
        {
            throw new NotImplementedException();
        }
    }
}