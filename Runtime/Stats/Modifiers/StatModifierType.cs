using System;

namespace ActionBuilder.Runtime
{
    public enum StatModifierType
    {
        /// <summary> 단순히 수치를 더하는 등의 연산. </summary>
        Additive = 0,
        
        /// <summary> 수치를 곱한다. </summary>
        Multiplicative = 1,
        
        /// <summary> 값을 덮어씌운다. </summary>
        Override = 2
    }
}