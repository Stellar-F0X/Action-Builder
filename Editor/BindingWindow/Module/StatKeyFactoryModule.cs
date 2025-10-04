using System;
using StatController.Runtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace StatController.Tool
{
    public class StatKeyFactoryModule : FactoryModule<IStatKey>
    {
        public StatKeyFactoryModule(string title, int layer = 1) : base(typeof(IStatKey), title, layer) { }
        
        protected override IStatKey Create(Type type, Vector2 position, string entryName)
        {
            object keyObject = Activator.CreateInstance(type);
            
            Assert.IsNotNull(keyObject);
            Assert.IsTrue(keyObject is IStatKey);

            return (IStatKey)keyObject;
        }
    }
}