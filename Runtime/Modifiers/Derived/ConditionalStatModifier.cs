using System;

namespace StatController.Runtime
{
    [Serializable]
    public struct ConditionalStatModifier : IStatModifier
    {
        public ConditionalStatModifier(string identity, float rightOperand, int priority = 0, StatModifierType modifierType = StatModifierType.Override, Func<ConditionalStatModifier, float, bool> condition = null)
        {
            _rightOperand = rightOperand;
            _condition = condition;

            this.modifierType = modifierType;
            this.identity = identity;
            this.priority = priority;
        }

        public ConditionalStatModifier(float rightOperand, int priority = 0, StatModifierType modifierType = StatModifierType.Override, Func<ConditionalStatModifier, float, bool> condition = null)
        {
            _rightOperand = rightOperand;
            _condition = condition;

            this.modifierType = modifierType;
            this.identity = string.Empty;
            this.priority = priority;
        }

        
        private event Func<ConditionalStatModifier, float, bool> _condition;

        private float _rightOperand;


        public StatModifierType modifierType
        {
            get;
            set;
        }

        public string identity
        {
            get;
            set;
        }

        public int priority
        {
            get;
            set;
        }

        float IStatModifier.rightOperand
        {
            get => _rightOperand;
            set => _rightOperand = value;
        }


        public float Calculate(float value)
        {
            if (_condition is null)
            {
                return value;
            }

            if (_condition.Invoke(this, value))
            {
                switch (modifierType)
                {
                    case StatModifierType.Override: return _rightOperand;

                    case StatModifierType.Additive: return value + _rightOperand;

                    case StatModifierType.Multiplicative: return value * _rightOperand;
                }
            }

            return value;
        }
    }
}