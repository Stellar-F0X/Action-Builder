using System;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class DurationOver : ConditionBase
    {
        public int durationIndex;
        
        public override bool CanExecute(ActionBase controller)
        {
            return true;
        }
    }
}