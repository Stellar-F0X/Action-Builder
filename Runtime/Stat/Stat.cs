using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace StatController.Runtime
{
    [Serializable]
    public class Stat : ICloneable
    {
        public float value;
        
        [SerializeReference, HideInInspector]
        private List<IStatModifier> _modifiers = new List<IStatModifier>();
        
        
        public void AddModifiers(params IStatModifier[] modifier) { }
        
        
        public void AddModifier(IStatModifier modifier) { }


        public void RemoveModifier(IStatModifier modifier) { }


        public void RemoveModifier(string name) { }


        private void ApplyModifiers() { }
        
        
        public virtual object Clone()
        {
            Stat clonedStat = Activator.CreateInstance(this.GetType()) as Stat;
            Assert.IsNotNull(clonedStat);
            
            clonedStat._modifiers = this._modifiers.ToList();
            clonedStat.value = this.value;
            return clonedStat;
        }
    }
}