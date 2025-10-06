using System;

namespace StatController.Runtime
{
    [Serializable]
    public class StatModifier : StatModifierBase
    {
        public StatModifier(string name, float rightOperand, int priority = 0, StatModifierType modifierType = StatModifierType.Override) : base(name, rightOperand, priority, modifierType)
        {
            this._rightOperand = rightOperand;
            this.modifierType = modifierType;
            this.name = name;
            this.priority = priority;
        }

        public StatModifier(float rightOperand, int priority = 0, StatModifierType modifierType = StatModifierType.Override) : base(string.Empty, rightOperand, priority, modifierType)
        {
            this._rightOperand = rightOperand;
            this.modifierType = modifierType;
            this.name = string.Empty;
            this.priority = priority;
        }

        
        public override float Calculate(float leftValue)
        {
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