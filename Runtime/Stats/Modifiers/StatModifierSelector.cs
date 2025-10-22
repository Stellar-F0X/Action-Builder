using System;
using UnityEngine;

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
    }
}