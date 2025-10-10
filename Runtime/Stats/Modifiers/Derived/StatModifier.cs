using System;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class StatModifier : StatModifierBase
    {
        public StatModifier(string name, float operand, int priority = 0, StatModifierType type = StatModifierType.Override) : base(name, operand, priority, type)
        {
            this._operand = operand;
            this._type = type;
            this._name = name;
            this._priority = priority;
        }

        
        public override float Calculate(float leftValue)
        {
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