using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class SequentialStatModifier : StatModifierBase
    {
        public SequentialStatModifier(string name, float operand, StatModifier[] modifiers, int priority = 0) : base(name, operand, priority, StatModifierType.Override)
        {
            _modifiers = modifiers;
        }

        [SerializeField]
        private StatModifier[] _modifiers;


        public override float Calculate(float leftValue)
        {
            throw new System.NotImplementedException();
        }
    }
}