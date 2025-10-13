using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct ChargeData
    {
        public ChargeType chargeType;
        public float minChargeTime;
        public float maxChargeTime;
        public ChargeCompletionBehavior completionBehavior;

        [Range(0f, 1f)]
        public float minChargeThreshold;
    }
}