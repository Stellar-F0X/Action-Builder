using System;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class StatModifier : StatModifierBase
    {
        public StatModifier() { }

        
        public StatModifier(string name, float operand, int priority = 0, StatModifierType type = StatModifierType.Override) : base(name, operand, priority, type)
        {
            this._operand = operand;
            this._type = type;
            this._name = name;
            this._priority = priority;
        }
    }
}