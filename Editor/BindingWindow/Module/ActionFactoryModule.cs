using System;
using ActionBuilder.Runtime;
using UnityEngine;

namespace ActionBuilder.Tool
{
    internal class ActionFactoryModule : FactoryModule<ActionBase>
    {
        public ActionFactoryModule(string title, int layer = 1) : base(typeof(ActionBase), title, layer) { }
        
        protected override ActionBase Create(Type type, Vector2 position, string entryName)
        {
            throw new NotImplementedException();
        }
    }
}