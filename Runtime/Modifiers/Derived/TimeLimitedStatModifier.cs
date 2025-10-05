using System;

namespace StatController.Runtime
{
    [Serializable]
    public class TimeLimitedStatModifier : StatModifierBase
    {
        public TimeLimitedStatModifier(string name, float rightOperand, int priority, StatModifierType modifierType) : base(name, rightOperand, priority, modifierType) { }
        
        public override float Calculate(float leftValue)
        {
            return 1f;
        }
    }
}