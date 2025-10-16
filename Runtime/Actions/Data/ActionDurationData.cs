using System;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct ActionDurationData
    {
        public ActionDuration durationType;
        public float duration;
        public float cooldownTime;
    }
}