using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct StatModifierSelector
    {
        public string statKey;

        [SerializeReference, SubclassSelector]
        public StatModifierBase modifier;

        [HideInInspector]
        public string keyTypeName;
    }
}