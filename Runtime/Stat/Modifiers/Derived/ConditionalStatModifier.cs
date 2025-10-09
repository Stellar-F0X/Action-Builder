using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class ConditionalStatModifier : StatModifierBase
    {
        public ConditionalStatModifier(string name, float operand, int priority = 0, StatModifierType type = StatModifierType.Override, Func<ConditionalStatModifier, float, bool> condition = null) : base(name, operand, priority, type)
        {
            this._operand = operand;
            this._type = type;
            this._name = name;
            this._priority = priority;
            this.condition = condition;
        }

        public ConditionalStatModifier(float operand, int priority = 0, StatModifierType type = StatModifierType.Override, Func<ConditionalStatModifier, float, bool> condition = null) : base(string.Empty, operand, priority, type)
        {
            this._operand = operand;
            this._type = type;
            this._name = string.Empty;
            this._priority = priority;
            this.condition = condition;
        }

        public event Func<ConditionalStatModifier, float, bool> condition;


        public override float Calculate(float leftValue)
        {
            if (condition is null || condition.Invoke(this, leftValue) == false)
            {
                return leftValue;
            }

            switch (base._type)
            {
                case StatModifierType.Override: return _operand;

                case StatModifierType.Additive: return leftValue + _operand;

                case StatModifierType.Multiplicative: return leftValue * _operand;
            }

            return leftValue;
        }
    }
}