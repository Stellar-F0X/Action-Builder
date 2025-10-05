namespace StatController.Runtime
{
    public enum StatModifierType
    {
        /// <summary> 값을 덮어씌운다. </summary>
        Override,

        /// <summary> 단순히 수치를 더하는 등의 연산. </summary>
        Additive,

        /// <summary> 수치를 곱한다. </summary>
        Multiplicative
    }
}