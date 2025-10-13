using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class ModifierPair
    {
        [SerializeReference]
        public object key;
        
        [SerializeReference]
        public DefaultStatModifier modifier;
    }
}