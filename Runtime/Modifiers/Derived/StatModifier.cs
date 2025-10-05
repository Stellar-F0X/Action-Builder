using System;

namespace StatController.Runtime
{
    [Serializable]
    public struct StatModifier : IStatModifier
    {
        public StatModifier(string identity, float rightOperand, int priority = 0, StatModifierType modifierType = StatModifierType.Override)
        {
            this._rightOperand = rightOperand;
            this.modifierType = modifierType;
            this.identity = identity;
            this.priority = priority;
        }

        public StatModifier(float rightOperand, int priority = 0, StatModifierType modifierType = StatModifierType.Override)
        {
            this._rightOperand = rightOperand;
            this.modifierType = modifierType;
            this.identity = string.Empty;
            this.priority = priority;
        }

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
            switch (modifierType)
            {
                case StatModifierType.Override: return _rightOperand;

                case StatModifierType.Additive: return value + _rightOperand;

                case StatModifierType.Multiplicative: return value * _rightOperand;
            }

            return value;
        }
    }
}