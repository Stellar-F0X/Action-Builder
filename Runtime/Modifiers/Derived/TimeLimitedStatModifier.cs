using System;

namespace StatController.Runtime
{
    [Serializable]
    public class TimeLimitedStatModifier : StatModifierBase
    {
        public TimeLimitedStatModifier(string name, float operand, int priority, StatModifierType type) : base(name, operand, priority, type) { }
        
        public override float Calculate(float leftValue)
        {
            return 1f;
        }
    }
}