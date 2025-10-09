using System;
using ActionBuilder.Runtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Tool
{
    internal class StatFactoryModule : FactoryModule<Stat>
    {
        public StatFactoryModule(string title, int layer = 1) : base(typeof(Stat), title, layer) { }
        
        protected override Stat Create(Type type, Vector2 position, string entryName)
        {
            object keyObject = Activator.CreateInstance(type);
            
            Assert.IsNotNull(keyObject);
            Assert.IsTrue(keyObject is Stat);

            return (Stat)keyObject;
        }
    }
}