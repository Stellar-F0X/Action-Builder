using System;
using UnityEngine;

namespace StatController.Runtime
{
    [Serializable]
    public class ConditionalStatModifier : StatModifierBase
    {
        public ConditionalStatModifier(string name, float rightOperand, int priority = 0, StatModifierType modifierType = StatModifierType.Override, Func<ConditionalStatModifier, float, bool> condition = null) : base(name, rightOperand, priority, modifierType)
        {
            this._rightOperand = rightOperand;
            this.modifierType = modifierType;
            this.name = name;
            this.priority = priority;
            this.condition = condition;
        }

        public ConditionalStatModifier(float rightOperand, int priority = 0, StatModifierType modifierType = StatModifierType.Override, Func<ConditionalStatModifier, float, bool> condition = null) : base(string.Empty, rightOperand, priority, modifierType)
        {
            this._rightOperand = rightOperand;
            this.modifierType = modifierType;
            this.name = string.Empty;
            this.priority = priority;
            this.condition = condition;
        }

        public event Func<ConditionalStatModifier, float, bool> condition;


        public override float Calculate(float leftValue)
        {
            if (condition is null || condition.Invoke(this, leftValue) == false)
            {
                return leftValue;
            }

            switch (base.modifierType)
            {
                case StatModifierType.Override: return _rightOperand;

                case StatModifierType.Additive: return leftValue + _rightOperand;

                case StatModifierType.Multiplicative: return leftValue * _rightOperand;
            }

            return leftValue;
        }
    }
}