using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatController.Runtime
{
    [Serializable]
    public class Stat
    {
        public float value;
        
        [SerializeReference]
        private List<IStatModifier> _modifiers = new List<IStatModifier>();
        
        
        public void AddModifiers(params IStatModifier[] modifier) { }
        
        
        public void AddModifier(IStatModifier modifier) { }


        public void RemoveModifier(IStatModifier modifier) { }


        public void RemoveModifier(string name) { }


        private void ApplyModifiers() { } 
    }
}