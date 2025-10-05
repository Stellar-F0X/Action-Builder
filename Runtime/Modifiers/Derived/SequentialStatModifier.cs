using System;
using UnityEngine;

namespace StatController.Runtime
{
    [Serializable]
    public class SequentialStatModifier : StatModifierBase
    {
        public SequentialStatModifier(string name, float rightOperand, StatModifier[] modifiers, int priority = 0) : base(name, rightOperand, priority, StatModifierType.Override)
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