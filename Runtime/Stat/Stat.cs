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
        [SerializeField]
        protected float _value;
        
        [SerializeReference, ReadOnly]
        private List<IStatModifier> _modifiers = new List<IStatModifier>();


        public virtual float value
        {
            get { return _value; }
            set { _value = value; }
        }  
        
        
        public void AddModifiers(params IStatModifier[] modifier) { }
        
        
        public void AddModifier(IStatModifier modifier) { }


        public void RemoveModifier(IStatModifier modifier) { }


        public void RemoveModifier(string name) { }


        private void ApplyModifiers() { }
        
        
        public virtual object Clone()
        {
            Stat clonedStat = Activator.CreateInstance(this.GetType()) as Stat;
            Assert.IsNotNull(clonedStat, "Failed to create clone of Stat instance");
            
            clonedStat._modifiers = this._modifiers.ToList();
            clonedStat._value = this._value;
            
            return clonedStat;
        }
    }
}