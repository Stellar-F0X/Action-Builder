using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct ExecutionData
    {
        [Min(0)]
        public float duration;
        
        [Min(1)]
        public int applyCount;
        public float applyInterval;
    }
}