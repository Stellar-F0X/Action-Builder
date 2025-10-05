using System;
using UnityEngine;

namespace StatController.Runtime
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public ReadOnlyAttribute(bool isRuntimeOnly = false)
        {
            this.isRuntimeOnly = isRuntimeOnly;
        }
        
        public readonly bool isRuntimeOnly;
    }
}