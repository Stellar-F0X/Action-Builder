using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class StatModifierSelector
    {
        public string statKey;
        
        [HideInInspector]
        public string keyTypeName;

        [SerializeReference, SubclassSelector]
        public StatModifierBase modifier;


        public void ChangeModifier(StatModifierBase newModifier)
        {
            this.modifier = newModifier;
        }
    }
}