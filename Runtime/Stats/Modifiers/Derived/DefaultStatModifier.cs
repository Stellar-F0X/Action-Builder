using System;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class DefaultStatModifier : StatModifierBase
    {
        public DefaultStatModifier() { }

        
        public DefaultStatModifier(string name, float operand, int priority = 0, StatModifierType modifierType = StatModifierType.Override) : base(name, operand, priority, modifierType)
        {
            this.operand = operand;
            this.modifierType = modifierType;
            this.name = name;
            this.priority = priority;
        }
    }
}